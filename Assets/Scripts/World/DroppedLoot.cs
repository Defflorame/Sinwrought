using UnityEngine;
using Game.Items;

namespace Game.World
{
    /// <summary>
    /// Физика выпавшего мешка. Мешок остаётся обычным Rigidbody: PhysX сам усыпляет его в покое, замораживать
    /// его не нужно (замороженный завис бы, например, над открывшимся проходом). Компонент задаёт стартовую
    /// скорость, возвращает мешок, провалившийся в пустоту, и удаляет объект после подбора.
    /// Подбор — обычный ItemPickup, выпавшее не сохраняется.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(ItemPickup))]
    public class DroppedLoot : MonoBehaviour
    {
        [Min(1f)]
        [Tooltip("На сколько метров ниже точки появления мешок считается провалившимся.")]
        [SerializeField] private float fallLimit = 30f;

        private Rigidbody _body;
        private ItemPickup _pickup;
        private Vector3 _spawnPosition;
        private bool _rescued;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _pickup = GetComponent<ItemPickup>();

            // Не сквозь тонкий пол и стены; настройка в префабе не обязательна.
            _body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _spawnPosition = transform.position;
        }

        private void OnEnable()
        {
            _pickup.OnPickedUp += HandlePickedUp;
        }

        private void OnDisable()
        {
            _pickup.OnPickedUp -= HandlePickedUp;
        }

        /// <summary>Вызывается сразу после создания: что внутри мешка и с какой скоростью он вылетает.</summary>
        public void Initialize(ItemData item, int quantity, Vector3 velocity)
        {
            _pickup.Configure(item, quantity);
            _spawnPosition = transform.position;
            _body.linearVelocity = velocity;
        }

        // Update не подходит: проверка нужна только пока тело двигается, и привязана к физическому шагу.
        private void FixedUpdate()
        {
            if (_body.IsSleeping()) return;
            if (transform.position.y > _spawnPosition.y - fallLimit) return;

            if (_rescued)
            {
                Debug.LogWarning($"DroppedLoot: {name} снова провалился под карту — удалён.", this);
                Destroy(gameObject);
                return;
            }

            _rescued = true;
            transform.position = _spawnPosition;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
        }

        private void HandlePickedUp()
        {
            Destroy(gameObject);
        }
    }
}