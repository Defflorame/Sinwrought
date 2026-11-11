using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Преследование. Идёт к цели (если не видит — к последней известной точке), на дистанции боя
    /// останавливается и поворачивается к цели, когда доступна атака — переходит в Attack.
    /// Потерял цель — Investigate (если враг не в режиме постоянного преследования).
    /// </summary>
    public class EnemyChaseState : IEnemyState
    {
        private const float RepathInterval = 0.2f;

        private readonly EnemyBrain _brain;
        private readonly EnemyContext _ctx;

        private float _repathTimer;

        public EnemyChaseState(EnemyBrain brain, EnemyContext ctx)
        {
            _brain = brain;
            _ctx = ctx;
        }

        public void Enter()
        {
            _repathTimer = 0f;
        }

        public void Tick()
        {
            if (!_ctx.HasTarget || _ctx.TargetIsDead)
            {
                _brain.GoIdle();
                return;
            }

            bool persistent = _brain.IsPursuitPersistent;
            bool canSee = _ctx.Perception.CanSeeTarget;

            if (!persistent && !_ctx.Perception.IsAware)
            {
                _brain.GoInvestigate(_ctx.Perception.HasLastKnownPosition ? _ctx.Perception.LastKnownPosition : _ctx.Target.position);
                return;
            }

            float distance = _ctx.FlatDistanceToTarget;

            if (_ctx.Combat.TrySelectAttack(distance, canSee, out EnemyAttackOption option))
            {
                _brain.GoAttack(option);
                return;
            }

            bool inEngageRange = distance <= _ctx.Combat.EngageDistance && (canSee || distance < 1.5f);
            if (inEngageRange)
            {
                _ctx.Movement.Stop();
                _ctx.Movement.FaceTowards(_ctx.Target.position);
                return;
            }

            _repathTimer -= Time.deltaTime;
            if (_repathTimer > 0f) return;
            _repathTimer = RepathInterval;

            Vector3 destination = persistent || canSee ? _ctx.Target.position : _ctx.Perception.LastKnownPosition;
            _ctx.Movement.MoveTo(destination, _ctx.Data.chaseSpeedMultiplier, Mathf.Max(0.3f, _ctx.Combat.EngageDistance * 0.9f));
        }

        public void Exit()
        {
        }
    }
}
