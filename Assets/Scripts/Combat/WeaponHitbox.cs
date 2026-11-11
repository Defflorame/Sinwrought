using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Отвечает только за обнаружение попадания оружия по цели.
    /// НЕ рассчитывает урон — это задача DamageResolver.
    /// Цепочка: WeaponHitbox → Target detected → DamagePacket → DamageResolver → DamageReceiver.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeaponHitbox : MonoBehaviour
    {
        [Tooltip("Слои целей, по которым это оружие наносит урон. Игрок: Enemy. Враг: Player. Слой с разрушаемыми предметами добавляйте сюда же. По умолчанию — все слои (как раньше).")]
        [SerializeField] private LayerMask hostileLayers = ~0;
        private Collider _collider;
        private GameObject _owner;
        private Stats.CharacterStats _ownerStats;
        private DamageReceiver _ownerReceiver;

        private AttackRuntime _currentAttack;
        private bool _isActive;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        /// <summary>Владелец оружия (источник урона) — игрок или враг. CharacterStats кэшируется здесь один раз, а не ищется при каждом попадании (см. раздел 76, п.12).</summary>
        public void SetOwner(GameObject owner)
        {
            _owner = owner;
            _ownerStats = owner.GetComponentInParent<Stats.CharacterStats>();
            _ownerReceiver = owner.GetComponentInParent<DamageReceiver>();
        }

        /// <summary>
        /// Растягивает коллайдер по трём осям под конкретную атаку. Ось,
        /// равная 0 в attackSize, сохраняет текущее значение коллайдера
        /// (X/Y) или берёт weaponRange как фолбэк (Z) — это позволяет
        /// старым AttackData без заполненного hitboxSize работать как
        /// раньше (только length от WeaponData.range).
        /// </summary>
        public void ApplyShape(Vector3 attackSize, float weaponRange)
        {
            switch (_collider)
            {
                case BoxCollider box:
                    Vector3 size = box.size;
                    if (attackSize.x > 0f) size.x = attackSize.x;
                    if (attackSize.y > 0f) size.y = attackSize.y;
                    size.z = attackSize.z > 0f ? attackSize.z : Mathf.Max(0.01f, weaponRange);
                    box.size = size;

                    Vector3 center = box.center;
                    center.z = size.z * 0.5f;
                    box.center = center;
                    break;

                case CapsuleCollider capsule:
                    if (attackSize.x > 0f) capsule.radius = attackSize.x * 0.5f;
                    capsule.height = attackSize.z > 0f ? attackSize.z : Mathf.Max(capsule.radius * 2f, weaponRange);

                    Vector3 capsuleCenter = capsule.center;
                    capsuleCenter.z = capsule.height * 0.5f;
                    capsule.center = capsuleCenter;
                    break;

                default:
                    Debug.LogWarning($"WeaponHitbox: ApplyShape поддерживает только BoxCollider/CapsuleCollider, у {name} другой тип коллайдера.", this);
                    break;
            }
        }

        public void Activate(AttackRuntime attackRuntime)
        {
            _currentAttack = attackRuntime;
            _isActive = true;
            _collider.enabled = true;
        }

        public void Deactivate()
        {
            _isActive = false;
            _collider.enabled = false;
            _currentAttack = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive || _currentAttack == null) return;
            if (!hostileLayers.Contains(other.gameObject.layer)) return;

            DamageReceiver receiver = other.GetComponentInParent<DamageReceiver>();
            if (receiver == null) return;
            if (receiver == _ownerReceiver) return;
            if (_currentAttack.AlreadyHitTargets.Contains(receiver)) return;

            _currentAttack.AlreadyHitTargets.Add(receiver);

            var packet = new DamagePacket(
            source: _owner,
            target: receiver.gameObject,
            basePhysicalDamage: _currentAttack.AttackData.damage,
            damageMultiplier: _currentAttack.DamageMultiplier,
            attackData: _currentAttack.AttackData,
            attackerStats: _ownerStats,
            element: _currentAttack.Element,
            elementalDamage: _currentAttack.ElementalDamage,
            onHitEffects: _currentAttack.OnHitEffects);

            DamageResolver.ResolveDamage(packet, receiver);
        }
    }
}