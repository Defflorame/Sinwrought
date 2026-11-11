using System;
using UnityEngine;
using Game.Combat;

namespace Game.Enemies
{
    /// <summary>
    /// Зрение и слух врага. Только определяет, ЗАМЕЧЕНА ли цель и где она последний раз была;
    /// что с этим делать — решает EnemyBrain. Проверка зрения раз в checkInterval секунд.
    /// Формулы дальности/угла/прямой видимости — в VisibilityService.
    /// </summary>
    public class EnemyPerception : MonoBehaviour
    {
        [Tooltip("Необязательно. Точка «глаз». Пусто — transform.position + 1.5 м вверх.")]
        [SerializeField] private Transform eye;
        [Tooltip("Слои, которые могут закрывать обзор (стены, двери). Сама цель в расчёте участвует всегда.")]
        [SerializeField] private LayerMask sightMask = ~0;
        [SerializeField] private float checkInterval = 0.1f;
        [Tooltip("Смещение точки, в которую целится зрение, относительно pivot цели (у игрока pivot в центре).")]
        [SerializeField] private Vector3 targetAimOffset = Vector3.zero;

        private EnemyData _data;
        private Transform _target;
        private HealthComponent _targetHealth;

        private float _nextCheckTime;
        private float _lastCheckTime;
        private float _lastSeenTime;
        private float _visibleTime;

        /// <summary>Цель видна прямо сейчас (по последней проверке).</summary>
        public bool CanSeeTarget { get; private set; }

        /// <summary>Враг «знает» о цели: заметил её и ещё не потерял.</summary>
        public bool IsAware { get; private set; }

        public bool HasLastKnownPosition { get; private set; }
        public Vector3 LastKnownPosition { get; private set; }

        public event Action OnTargetSpotted;
        public event Action<Vector3> OnNoiseHeard;

        private Vector3 EyePosition => eye != null ? eye.position : transform.position + Vector3.up * 1.5f;

        public void Configure(EnemyData data)
        {
            _data = data;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            _targetHealth = target != null ? target.GetComponentInParent<HealthComponent>() : null;
            _lastCheckTime = Time.time;
            _nextCheckTime = Time.time;
        }

        private void OnEnable()
        {
            NoiseService.OnNoise += HandleNoise;
        }

        private void OnDisable()
        {
            NoiseService.OnNoise -= HandleNoise;
        }

        private void Update()
        {
            if (_data == null || _target == null) return;
            if (Time.time < _nextCheckTime) return;

            float elapsed = Mathf.Min(Time.time - _lastCheckTime, 0.5f);
            _lastCheckTime = Time.time;
            _nextCheckTime = Time.time + checkInterval;

            bool visible = EvaluateVisibility();
            CanSeeTarget = visible;

            if (visible)
            {
                _lastSeenTime = Time.time;
                LastKnownPosition = _target.position;
                HasLastKnownPosition = true;
                _visibleTime += elapsed;

                if (!IsAware && _visibleTime >= _data.reactionTime)
                {
                    IsAware = true;
                    OnTargetSpotted?.Invoke();
                }
            }
            else
            {
                _visibleTime = 0f;

                if (IsAware && Time.time - _lastSeenTime >= _data.loseTargetDelay)
                {
                    IsAware = false;
                }
            }
        }

        private bool EvaluateVisibility()
        {
            if (_targetHealth != null && _targetHealth.IsDead()) return false;

            Vector3 eyePosition = EyePosition;
            Vector3 targetPoint = _target.position + targetAimOffset;
            Vector3 toTarget = targetPoint - eyePosition;
            float distance = toTarget.magnitude;

            if (distance > _data.closeAwarenessRadius)
            {
                Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
                Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                float angle = flatToTarget.sqrMagnitude > 0.0001f && flatForward.sqrMagnitude > 0.0001f
                    ? Vector3.Angle(flatForward, flatToTarget)
                    : 0f;

                float range = _data.sightRange
                    * VisibilityService.GetSightMultiplier(transform.position)
                    * VisibilityService.GetRangeFactor(angle, _data.fieldOfView, _data.sideRangeMultiplier, _data.rearRangeMultiplier);

                if (distance > range) return false;
            }

            return VisibilityService.HasLineOfSight(eyePosition, targetPoint, _target, transform, sightMask);
        }

        private void HandleNoise(Vector3 position, float loudness, GameObject source)
        {
            if (_data == null || source == gameObject) return;

            float hearingRange = loudness * _data.hearingSensitivity;
            if ((position - transform.position).sqrMagnitude > hearingRange * hearingRange) return;

            OnNoiseHeard?.Invoke(position);
        }

        private void OnDrawGizmosSelected()
        {
            if (_data == null) return;

            Vector3 origin = EyePosition;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float baseRange = _data.sightRange * VisibilityService.GetSightMultiplier(transform.position);
            float half = _data.fieldOfView * 0.5f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + Quaternion.AngleAxis(half, Vector3.up) * forward * baseRange);
            Gizmos.DrawLine(origin, origin + Quaternion.AngleAxis(-half, Vector3.up) * forward * baseRange);
            Gizmos.DrawLine(origin, origin - forward * baseRange * _data.rearRangeMultiplier);

            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, _data.closeAwarenessRadius);
        }
    }
}
