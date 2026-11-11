using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemies
{
    /// <summary>
    /// Патруль: по waypoints экземпляра (по кругу или туда-обратно), а если их нет —
    /// по случайным точкам в EnemyData.patrolRadius вокруг стартовой точки.
    /// В каждой точке стоит patrolWaitMin–Max секунд.
    /// </summary>
    public class EnemyPatrolState : IEnemyState
    {
        private const float MaxMoveTime = 30f;

        private readonly EnemyContext _ctx;

        private int _waypointIndex = -1;
        private int _waypointDirection = 1;
        private bool _waiting;
        private float _timer;
        private float _waitDuration;

        public EnemyPatrolState(EnemyContext ctx)
        {
            _ctx = ctx;
        }

        public void Enter()
        {
            GoToNextPoint();
        }

        public void Tick()
        {
            _timer += Time.deltaTime;

            if (_waiting)
            {
                if (_timer >= _waitDuration) GoToNextPoint();
                return;
            }

            if (_ctx.Movement.HasArrived || _timer > MaxMoveTime)
            {
                StartWaiting();
            }
        }

        public void Exit()
        {
            _ctx.Movement.Stop();
        }

        private void StartWaiting()
        {
            _waiting = true;
            _timer = 0f;
            _waitDuration = Random.Range(_ctx.Data.patrolWaitMin, Mathf.Max(_ctx.Data.patrolWaitMin, _ctx.Data.patrolWaitMax));
            _ctx.Movement.Stop();
        }

        private void GoToNextPoint()
        {
            _timer = 0f;
            _waiting = false;

            if (!TryGetNextPoint(out Vector3 point) || !_ctx.Movement.MoveTo(point, _ctx.Data.patrolSpeedMultiplier))
            {
                StartWaiting();
            }
        }

        private bool TryGetNextPoint(out Vector3 point)
        {
            point = default;

            if (_ctx.Waypoints != null && _ctx.Waypoints.Length > 0)
            {
                AdvanceWaypointIndex();
                Transform waypoint = _ctx.Waypoints[_waypointIndex];
                if (waypoint == null) return false;

                point = waypoint.position;
                return true;
            }

            float radius = _ctx.Data.patrolRadius;
            if (radius <= 0.5f) return false;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                Vector3 candidate = _ctx.HomePosition + new Vector3(offset.x, 0f, offset.y);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            return false;
        }

        private void AdvanceWaypointIndex()
        {
            int count = _ctx.Waypoints.Length;

            if (_waypointIndex < 0)
            {
                _waypointIndex = 0;
                return;
            }

            if (_ctx.PingPongPatrol && count > 1)
            {
                int next = _waypointIndex + _waypointDirection;
                if (next >= count || next < 0)
                {
                    _waypointDirection = -_waypointDirection;
                    next = _waypointIndex + _waypointDirection;
                }

                _waypointIndex = next;
            }
            else
            {
                _waypointIndex = (_waypointIndex + 1) % count;
            }
        }
    }
}
