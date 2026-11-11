using UnityEngine;

namespace Game.Enemies
{
    /// <summary>Враг оглушён: не двигается и не атакует stagger-время, потом продолжает бой.</summary>
    public class EnemyStaggerState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private readonly EnemyContext _ctx;

        private float _duration;
        private float _timer;

        public EnemyStaggerState(EnemyBrain brain, EnemyContext ctx)
        {
            _brain = brain;
            _ctx = ctx;
        }

        public void SetDuration(float duration)
        {
            _duration = duration;
        }

        public void Enter()
        {
            _timer = 0f;
            _ctx.Movement.Stop();
            _ctx.Combat.CancelAttack();
        }

        public void Tick()
        {
            _timer += Time.deltaTime;
            if (_timer >= _duration) _brain.GoAfterInterrupt();
        }

        public void Exit()
        {
        }
    }
}
