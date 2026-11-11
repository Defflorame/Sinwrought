using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Стоит на месте. Если враг далеко от своей стартовой точки (после погони/расследования) —
    /// сначала возвращается домой. Потом ждёт и, если есть патруль, начинает его.
    /// </summary>
    public class EnemyIdleState : IEnemyState
    {
        private const float HomeTolerance = 1.5f;

        private readonly EnemyBrain _brain;
        private readonly EnemyContext _ctx;

        private float _timer;
        private bool _returningHome;

        public EnemyIdleState(EnemyBrain brain, EnemyContext ctx)
        {
            _brain = brain;
            _ctx = ctx;
        }

        public void Enter()
        {
            _timer = Random.Range(_ctx.Data.idleTimeMin, Mathf.Max(_ctx.Data.idleTimeMin, _ctx.Data.idleTimeMax));

            Vector3 toHome = _ctx.HomePosition - _ctx.Self.position;
            toHome.y = 0f;
            _returningHome = toHome.magnitude > HomeTolerance
                && _ctx.Movement.MoveTo(_ctx.HomePosition, _ctx.Data.patrolSpeedMultiplier);

            if (!_returningHome) _ctx.Movement.Stop();
        }

        public void Tick()
        {
            if (_returningHome)
            {
                if (_ctx.Movement.HasArrived)
                {
                    _returningHome = false;
                    _ctx.Movement.Stop();
                }

                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            if (_brain.HasPatrol)
            {
                _brain.GoPatrol();
            }
            else
            {
                _timer = Random.Range(_ctx.Data.idleTimeMin, Mathf.Max(_ctx.Data.idleTimeMin, _ctx.Data.idleTimeMax));
            }
        }

        public void Exit()
        {
            _ctx.Movement.Stop();
        }
    }
}
