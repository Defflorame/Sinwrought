using UnityEngine;
using Game.Combat;

namespace Game.Enemies
{
    /// <summary>
    /// «Мозг» врага: реагирует на события (заметил цель, услышал шум, получил удар, оглушён,
    /// умер) и переключает EnemyStateMachine. Движением, зрением и боем сам не занимается —
    /// только решает, в каком состоянии сейчас враг.
    /// </summary>
    public class EnemyBrain : MonoBehaviour
    {
        private EnemyContext _ctx;
        private EnemyStateMachine _machine;
        private PoiseTracker _poise;
        private EnemyIdleState _idle;
        private EnemyPatrolState _patrol;
        private EnemyInvestigateState _investigate;
        private EnemyChaseState _chase;
        private EnemyAttackState _attack;
        private EnemyStaggerState _stagger;
        private EnemyDeadState _dead;

        private bool _hasSpottedTarget;

        public bool IsDead { get; private set; }

        /// <summary>Устойчивость к оглушению (null до Begin). Только для чтения.</summary>
        public PoiseTracker Poise => _poise;

        public bool HasPatrol =>
            (_ctx.Waypoints != null && _ctx.Waypoints.Length > 0) || _ctx.Data.patrolRadius > 0.5f;

        /// <summary>Враг не теряет цель (режим экземпляра на сцене).</summary>
        public bool IsPursuitPersistent =>
            _ctx.Pursuit == EnemyPursuitMode.AlwaysHunting
            || (_ctx.Pursuit == EnemyPursuitMode.NeverLoseTarget && _hasSpottedTarget);

        private bool IsPassive =>
            _machine.CurrentState == _idle || _machine.CurrentState == _patrol || _machine.CurrentState == _investigate;

        public void Begin(EnemyContext ctx, EnemyStateMachine machine)
        {
            _ctx = ctx;
            _machine = machine;
            _poise = new PoiseTracker(ctx.Stats, ctx.Data.poiseResetDelay);
            _idle = new EnemyIdleState(this, ctx);
            _patrol = new EnemyPatrolState(ctx);
            _investigate = new EnemyInvestigateState(this, ctx);
            _chase = new EnemyChaseState(this, ctx);
            _attack = new EnemyAttackState(this, ctx);
            _stagger = new EnemyStaggerState(this, ctx);
            _dead = new EnemyDeadState(ctx);

            ctx.Perception.OnTargetSpotted += HandleTargetSpotted;
            ctx.Perception.OnNoiseHeard += HandleNoiseHeard;
            ctx.Receiver.OnStaggerReceived += HandleStaggerReceived;
            ctx.Receiver.OnParried += HandleParried;
            ctx.Receiver.OnDamaged += HandleDamaged;
            ctx.Health.OnDeath += HandleDeath;
            if (ctx.TargetHealth != null) ctx.TargetHealth.OnDeath += HandleTargetDied;

            if (!ctx.HasTarget)
            {
                Debug.LogWarning($"EnemyBrain: у {name} не назначена цель (EnemyController.target) — враг будет стоять.", this);
            }

            if (ctx.Pursuit == EnemyPursuitMode.AlwaysHunting && ctx.HasTarget)
            {
                GoChase();
            }
            else
            {
                GoIdle();
            }
        }

        private void OnDestroy()
        {
            if (_ctx == null) return;

            _ctx.Perception.OnTargetSpotted -= HandleTargetSpotted;
            _ctx.Perception.OnNoiseHeard -= HandleNoiseHeard;
            _ctx.Receiver.OnStaggerReceived -= HandleStaggerReceived;
            _ctx.Receiver.OnParried -= HandleParried;
            _ctx.Receiver.OnDamaged -= HandleDamaged;
            _ctx.Health.OnDeath -= HandleDeath;
            if (_ctx.TargetHealth != null) _ctx.TargetHealth.OnDeath -= HandleTargetDied;
        }

        // ---------- Переходы (их вызывают и состояния) ----------

        public void GoIdle()
        {
            if (IsDead) return;
            _machine.ChangeState(_idle);
        }

        public void GoPatrol()
        {
            if (IsDead) return;
            _machine.ChangeState(_patrol);
        }

        public void GoChase()
        {
            if (IsDead) return;

            if (!_ctx.HasTarget || _ctx.TargetIsDead)
            {
                GoIdle();
                return;
            }

            _machine.ChangeState(_chase);
        }

        public void GoInvestigate(Vector3 point)
        {
            if (IsDead) return;
            _investigate.SetPoint(point);
            _machine.ChangeState(_investigate);
        }

        public void GoAttack(EnemyAttackOption option)
        {
            if (IsDead) return;
            _attack.SetOption(option);
            _machine.ChangeState(_attack);
        }

        public void GoStagger(float duration)
        {
            if (IsDead) return;
            _stagger.SetDuration(duration);
            _machine.ChangeState(_stagger);
        }

        /// <summary>Что делать после оглушения: враг знает, где игрок — продолжает бой, иначе идёт проверить.</summary>
        public void GoAfterInterrupt()
        {
            if (!_ctx.HasTarget || _ctx.TargetIsDead)
            {
                GoIdle();
            }
            else if (_ctx.Perception.IsAware || IsPursuitPersistent)
            {
                GoChase();
            }
            else
            {
                GoInvestigate(_ctx.Target.position);
            }
        }

        // ---------- Реакции на события ----------

        private void HandleTargetSpotted()
        {
            if (IsDead) return;

            _hasSpottedTarget = true;
            if (IsPassive) GoChase();
        }

        private void HandleNoiseHeard(Vector3 position)
        {
            if (IsDead || !IsPassive) return;
            GoInvestigate(position);
        }

        private void HandleDamageTaken(int amount)
        {
            if (IsDead || !_ctx.HasTarget || !IsPassive) return;
            GoInvestigate(_ctx.Target.position);
        }

        private void HandleStaggerReceived(float staggerPower)
        {
            if (IsDead || _ctx.Data.immuneToStagger) return;

            float powerDivisor = _machine.CurrentState == _attack ? _ctx.Data.attackingPoiseMultiplier : 1f;
            if (_poise.Absorb(staggerPower, powerDivisor))
            {
                GoStagger(_ctx.Data.staggerDuration);
            }
        }

        private void HandleParried()
        {
            if (IsDead || _ctx.Data.immuneToStagger) return;
            GoStagger(_ctx.Data.parryStaggerDuration);
        }

        private void HandleDeath()
        {
            if (IsDead) return;

            IsDead = true;
            _machine.ChangeState(_dead);
        }

        private void HandleTargetDied()
        {
            GoIdle();
        }

        private void HandleDamaged(DamagePacket packet)
        {
            // Тик яда/ожога (Source == null) врага не наводит: он не знает, откуда урон.
            // Идём к месту, откуда прилетел удар, а не к текущей позиции игрока.
            if (IsDead || !IsPassive || packet.Source == null) return;
            GoInvestigate(packet.Source.transform.position);
        }
    }
}
