using UnityEngine;
using Game.Player.Stats;
using Game.Player.Equipment;
using Game.Magic;
using Game.Stats;

namespace Game.Player
{
    /// <summary>
    /// Горизонтальное перемещение игрока. Не смешивается с боевой системой.
    /// Ускоренный бег отсутствует. Базовая скорость берётся из CharacterStats
    /// (характеристика Speed). С Phase 4 currentWeight приходит из
    /// PlayerEquipment.GetTotalWeight() — раньше здесь была заглушка на 0,
    /// т.к. Equipment ещё не существовал.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour, ILaunchable
    {
        [SerializeField] private CharacterStats characterStats;
        [SerializeField] private PlayerEquipment playerEquipment;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpHeight = 1.2f;

        private CharacterController _controller;
        private Vector2 _currentInput;
        private float _verticalVelocity;
        private bool _movementLocked;
        private float _effectiveSpeed;

        private bool _isDashing;
        private Vector3 _dashDirection;
        private float _dashSpeed;

        public bool IsGrounded => _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (characterStats != null)
            {
                characterStats.OnStatsChanged += HandleStatsChanged;
            }

            if (playerEquipment != null)
            {
                playerEquipment.OnEquipmentChanged += HandleEquipmentChanged;
            }
        }

        private void OnDisable()
        {
            if (characterStats != null)
            {
                characterStats.OnStatsChanged -= HandleStatsChanged;
            }

            if (playerEquipment != null)
            {
                playerEquipment.OnEquipmentChanged -= HandleEquipmentChanged;
            }
        }

        private void Start()
        {
            RecalculateSpeed();
        }

        private void Update()
        {
            if (_movementLocked && !_isDashing)
            {
                _currentInput = Vector2.zero;
            }

            ApplyGravity();

            Vector3 horizontalVelocity;
            if (_isDashing)
            {
                horizontalVelocity = _dashDirection * _dashSpeed;
            }
            else
            {
                Vector3 moveDirection = transform.right * _currentInput.x + transform.forward * _currentInput.y;
                horizontalVelocity = moveDirection * _effectiveSpeed;
            }

            Vector3 velocity = horizontalVelocity;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        public void Move(Vector2 input)
        {
            _currentInput = input;
        }

        public void StopMovement()
        {
            _currentInput = Vector2.zero;
        }

        public Vector3 GetVelocity()
        {
            return _controller.velocity;
        }

        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;
            if (locked)
            {
                _currentInput = Vector2.zero;
            }
        }
        /// <summary>
        /// Мгновенный перенос игрока (вход в локацию, телепорт). CharacterController на время переноса
        /// отключается, иначе он перезапишет позицию. Поворот берётся только по yaw (без наклона тела).
        /// Накопленная скорость падения и ввод сбрасываются.
        /// </summary>
        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            _controller.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));
            _controller.enabled = true;

            _verticalVelocity = 0f;
            _currentInput = Vector2.zero;
        }

        public void BeginDash(Vector3 worldDirection, float speed)
        {
            _isDashing = true;
            _dashDirection = worldDirection.normalized;
            _dashSpeed = speed;
        }

        public void EndDash()
        {
            _isDashing = false;
        }

        public void Jump()
        {
            if (_movementLocked || !_controller.isGrounded) return;

            _verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }

        /// <summary>
        /// Мгновенный подброс на заданную высоту (заклинание прыжка). В отличие от Jump(),
        /// не требует стоять на земле — ограничением служат мана и кулдаун заклинания.
        /// </summary>
        public void Launch(float height)
        {
            if (_movementLocked) return;

            _verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * Mathf.Max(0f, height));
        }

        private void HandleStatsChanged(DerivedCharacterStats stats)
        {
            RecalculateSpeed();
        }

        private void HandleEquipmentChanged()
        {
            RecalculateSpeed();
        }

        private void RecalculateSpeed()
        {
            if (characterStats == null)
            {
                Debug.LogWarning("PlayerMovement: CharacterStats не назначен — используется скорость по умолчанию.", this);
                _effectiveSpeed = 5f;
                return;
            }
            float loadRatio = playerEquipment != null ? playerEquipment.GetLoadRatio() : 0f;
            _effectiveSpeed = characterStats.Stats.MovementSpeed * EncumbranceService.GetMovementMultiplier(loadRatio);
        }
    }
}