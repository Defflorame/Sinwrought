using UnityEngine;
using UnityEngine.AI;
using Game.Player.Stats;
using Game.Stats;

namespace Game.Enemies
{
    /// <summary>
    /// Обёртка над NavMeshAgent: куда идти, насколько быстро и куда смотреть.
    /// Скорость = характеристика Speed из CharacterStats × множитель состояния
    /// (замедляющие статус-эффекты работают сами). Поворот делает этот компонент, а не агент.
    /// Ничего не знает про бой и восприятие.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        private const float FallbackSpeed = 3.5f;

        [Tooltip("Необязательно. Без него скорость — 3.5 м/с.")]
        [SerializeField] private CharacterStats characterStats;

        private NavMeshAgent _agent;
        private float _speedMultiplier = 1f;
        private float _turnSpeed = 360f;
        private bool _hasFaceRequest;
        private Vector3 _faceDirection;

        private NavMeshAgent Agent => _agent != null ? _agent : (_agent = GetComponent<NavMeshAgent>());

        public bool IsOnNavMesh => Agent.isOnNavMesh;

        public void Configure(EnemyData data)
        {
            _turnSpeed = data.turnSpeed;
            Agent.updateRotation = false;
            Agent.baseOffset = data.hoverHeight;
            RefreshSpeed();
        }

        private void OnEnable()
        {
            if (characterStats != null) characterStats.OnStatsChanged += HandleStatsChanged;
        }

        private void OnDisable()
        {
            if (characterStats != null) characterStats.OnStatsChanged -= HandleStatsChanged;
        }

        private void Start()
        {
            RefreshSpeed();

            if (!Agent.isOnNavMesh)
            {
                Debug.LogWarning($"EnemyMovement: {name} стоит не на NavMesh — запеките NavMesh или передвиньте врага.", this);
            }
        }

        /// <summary>false — путь построить нельзя (нет NavMesh рядом с точкой).</summary>
        public bool MoveTo(Vector3 worldPosition, float speedMultiplier, float stoppingDistance = 0.3f)
        {
            if (!Agent.isOnNavMesh) return false;
            if (!NavMesh.SamplePosition(worldPosition, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) return false;

            _speedMultiplier = speedMultiplier;
            RefreshSpeed();

            Agent.stoppingDistance = stoppingDistance;
            Agent.isStopped = false;
            return Agent.SetDestination(hit.position);
        }

        public void Stop()
        {
            if (!Agent.isOnNavMesh) return;

            Agent.isStopped = true;
            Agent.ResetPath();
            Agent.velocity = Vector3.zero; // иначе агент тормозит по acceleration и «проезжает» дальше
        }

        public bool HasArrived
        {
            get
            {
                if (!Agent.isOnNavMesh) return true;
                if (Agent.pathPending) return false;
                if (Agent.remainingDistance > Agent.stoppingDistance + 0.1f) return false;

                return !Agent.hasPath || Agent.velocity.sqrMagnitude < 0.01f;
            }
        }

        /// <summary>Повернуться к точке в этом кадре (горизонтально). Вызывать каждый кадр, пока нужно.</summary>
        public void FaceTowards(Vector3 worldPoint)
        {
            FaceDirection(worldPoint - transform.position);
        }

        /// <summary>Повернуться в направлении в этом кадре. Вызывать каждый кадр, пока нужно.</summary>
        public void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            _hasFaceRequest = true;
            _faceDirection = direction;
        }

        public void Disable()
        {
            if (Agent.isOnNavMesh) Agent.isStopped = true;
            Agent.enabled = false;
            enabled = false;
        }

        private void LateUpdate()
        {
            Vector3 direction = Vector3.zero;

            if (_hasFaceRequest)
            {
                direction = _faceDirection;
            }
            else if (Agent.enabled && Agent.velocity.sqrMagnitude > 0.04f)
            {
                direction = Agent.velocity;
            }

            _hasFaceRequest = false;

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }

        private void HandleStatsChanged(DerivedCharacterStats stats)
        {
            RefreshSpeed();
        }

        private void RefreshSpeed()
        {
            float baseSpeed = characterStats != null && characterStats.Stats != null
                ? characterStats.Stats.MovementSpeed
                : FallbackSpeed;

            Agent.speed = baseSpeed * _speedMultiplier;
        }
    }
}
