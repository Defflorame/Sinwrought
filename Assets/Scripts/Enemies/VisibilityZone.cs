using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Зона с изменённой видимостью (например, плотный туман — позже). Только данные + форма:
    /// коллайдер (Box/Sphere/Capsule или выпуклый Mesh), isTrigger включается сам.
    /// Все враги внутри зоны видят на sightMultiplier от своей дальности зрения.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VisibilityZone : MonoBehaviour
    {
        [Tooltip("Множитель дальности зрения врагов внутри зоны. 1 — без изменений, 0.3 — густой туман.")]
        [Range(0.05f, 2f)]
        [SerializeField] private float sightMultiplier = 0.5f;

        private Collider _collider;

        public float SightMultiplier => sightMultiplier;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        private void OnEnable()
        {
            VisibilityService.Register(this);
        }

        private void OnDisable()
        {
            VisibilityService.Unregister(this);
        }

        public bool Contains(Vector3 point)
        {
            if (_collider == null || !_collider.enabled) return false;
            if (!_collider.bounds.Contains(point)) return false;

            return (_collider.ClosestPoint(point) - point).sqrMagnitude < 0.0001f;
        }
    }
}
