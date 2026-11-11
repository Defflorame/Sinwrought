using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Враг проводит выбранную атаку: стоит на месте, во время замаха следит за целью (так игрок
    /// успевает отреагировать на телеграф), на ударе фиксирует направление. Сам бой считает
    /// EnemyCombatController. При выходе (в том числе из-за оглушения) атака отменяется.
    /// </summary>
    public class EnemyAttackState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private readonly EnemyContext _ctx;

        private EnemyAttackOption _option;

        public EnemyAttackState(EnemyBrain brain, EnemyContext ctx)
        {
            _brain = brain;
            _ctx = ctx;
        }

        public void SetOption(EnemyAttackOption option)
        {
            _option = option;
        }

        public void Enter()
        {
            _ctx.Movement.Stop();
            _ctx.Combat.StartAttack(_option, _ctx.Target.position);
        }

        public void Tick()
        {
            _ctx.Combat.Tick(Time.deltaTime, _ctx.Target.position);

            if (_ctx.Combat.CurrentPhase == Game.Combat.AttackPhase.Startup)
            {
                _ctx.Movement.FaceTowards(_ctx.Target.position);
            }

            if (!_ctx.Combat.IsAttacking)
            {
                _brain.GoChase();
            }
        }

        public void Exit()
        {
            _ctx.Combat.CancelAttack();
        }
    }
}
