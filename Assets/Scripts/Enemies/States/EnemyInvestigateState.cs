using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Идёт в указанную точку (последнее место, где видел цель, или источник шума),
    /// осматривается lookAroundDuration секунд и возвращается в Idle (а тот уведёт врага домой).
    /// Если по пути заметил цель — EnemyBrain переведёт в Chase.
    /// </summary>
    public class EnemyInvestigateState : IEnemyState
    {
        private enum Phase { Moving, Looking }

        private readonly EnemyBrain _brain;
        private readonly EnemyContext _ctx;

        private Vector3 _point;
        private Phase _phase;
        private float _timer;
        private Vector3 _lookBaseDirection;

        public EnemyInvestigateState(EnemyBrain brain, EnemyContext ctx)
        {
            _brain = brain;
            _ctx = ctx;
        }

        public void SetPoint(Vector3 point)
        {
            _point = point;
        }

        public void Enter()
        {
            _phase = Phase.Moving;

            if (!_ctx.Movement.MoveTo(_point, _ctx.Data.investigateSpeedMultiplier, 0.5f))
            {
                StartLooking();
            }
        }

        public void Tick()
        {
            if (_phase == Phase.Moving)
            {
                if (_ctx.Movement.HasArrived) StartLooking();
                return;
            }

            _timer += Time.deltaTime;

            float duration = Mathf.Max(0.01f, _ctx.Data.lookAroundDuration);
            if (_timer >= duration)
            {
                _brain.GoIdle();
                return;
            }

            // Один полный «взгляд влево–вправо» за время осмотра.
            float angle = Mathf.Sin(_timer / duration * Mathf.PI * 2f) * _ctx.Data.lookAroundAngle;
            _ctx.Movement.FaceDirection(Quaternion.AngleAxis(angle, Vector3.up) * _lookBaseDirection);
        }

        public void Exit()
        {
            _ctx.Movement.Stop();
        }

        private void StartLooking()
        {
            _phase = Phase.Looking;
            _timer = 0f;
            _lookBaseDirection = Vector3.ProjectOnPlane(_ctx.Self.forward, Vector3.up);
            _ctx.Movement.Stop();
        }
    }
}
