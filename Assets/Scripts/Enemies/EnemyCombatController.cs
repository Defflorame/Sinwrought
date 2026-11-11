using System;
using UnityEngine;
using Game.Combat;
using Game.Items;
using Game.Magic;

namespace Game.Enemies
{
    /// <summary>
    /// Выбирает и проводит атаку врага: ближнюю (AttackData + общий WeaponHitbox → DamagePacket →
    /// DamageResolver) или заклинание (общий SpellEffectExecutor). Фазы Startup/Active/Recovery считаются
    /// в Tick (без корутин), тик вызывает EnemyAttackState. Урон не считает и о состояниях не знает.
    /// Для обратной связи (анимация, звук, подсветка замаха) только события.
    /// </summary>
    public class EnemyCombatController : MonoBehaviour
    {
        [Tooltip("Необязательно (нет у чистых магов). Триггер-коллайдер перед врагом.")]
        [SerializeField] private WeaponHitbox weaponHitbox;
        [Tooltip("Необязательно (нужен только для заклинаний). Висит на корне врага.")]
        [SerializeField] private SpellEffectExecutor spellExecutor;
        [Tooltip("Необязательно (нужен только для заклинаний). Точка, откуда вылетают заклинания; перед кастом разворачивается на цель.")]
        [SerializeField] private Transform castOrigin;

        private EnemyData _data;
        private float[] _nextReadyTime = Array.Empty<float>();
        private float _nextAttackAllowedTime;

        private EnemyAttackOption _current;
        private AttackRuntime _runtime;
        private AttackPhase _phase = AttackPhase.Done;
        private float _phaseTimer;
        private Vector3 _aimPoint;

        public bool IsAttacking => _phase != AttackPhase.Done;
        public AttackPhase CurrentPhase => _phase;
        public bool CurrentAttackCanBeParried => _current != null && _current.attack != null && _current.attack.canBeParried;

        /// <summary>Замах начался (можно показать телеграф: подсветка, звук, анимация).</summary>
        public event Action<EnemyAttackOption> OnWindupStarted;

        /// <summary>Удар/каст произошёл (началась активная фаза).</summary>
        public event Action OnStrike;

        /// <summary>Атака закончилась или была прервана.</summary>
        public event Action OnAttackEnded;

        /// <summary>Расстояние, на котором враг останавливается при преследовании.</summary>
        public float EngageDistance { get; private set; } = 1.5f;

        private WeaponData Weapon => _data != null ? _data.weapon : null;

        private void Awake()
        {
            if (weaponHitbox != null) weaponHitbox.SetOwner(gameObject);
        }

        public void Configure(EnemyData data)
        {
            _data = data;

            int count = data.attacks != null ? data.attacks.Length : 0;
            _nextReadyTime = new float[count];
            EngageDistance = ResolveEngageDistance();
        }

        /// <summary>Выбирает случайную (с учётом веса) доступную сейчас атаку. false — выбрать нечего.</summary>
        public bool TrySelectAttack(float distance, bool hasLineOfSight, out EnemyAttackOption selected)
        {
            selected = null;
            if (_data == null || _data.attacks == null || IsAttacking) return false;
            if (Time.time < _nextAttackAllowedTime) return false;

            float totalWeight = 0f;
            for (int i = 0; i < _data.attacks.Length; i++)
            {
                if (IsAvailable(i, distance, hasLineOfSight)) totalWeight += _data.attacks[i].weight;
            }

            if (totalWeight <= 0f) return false;

            float roll = UnityEngine.Random.value * totalWeight;
            for (int i = 0; i < _data.attacks.Length; i++)
            {
                if (!IsAvailable(i, distance, hasLineOfSight)) continue;

                selected = _data.attacks[i];
                roll -= selected.weight;
                if (roll <= 0f) break;
            }

            return selected != null;
        }

        public void StartAttack(EnemyAttackOption option, Vector3 aimPoint)
        {
            if (option == null || IsAttacking) return;

            int index = Array.IndexOf(_data.attacks, option);
            if (index >= 0) _nextReadyTime[index] = Time.time + option.cooldown;

            _current = option;
            _aimPoint = aimPoint;
            _phase = AttackPhase.Startup;
            _phaseTimer = 0f;

            if (option.spell == null)
            {
                float weaponRange = Weapon != null ? Weapon.range : 1.5f;
                if (weaponHitbox != null) weaponHitbox.ApplyShape(option.attack.hitboxSize, weaponRange);
                _runtime = new AttackRuntime(option.attack, Weapon, Time.time, 1f);
            }

            OnWindupStarted?.Invoke(option);
        }

        /// <summary>Продвигает фазы атаки. aimPoint — где сейчас цель (нужно заклинаниям).</summary>
        public void Tick(float deltaTime, Vector3 aimPoint)
        {
            if (!IsAttacking) return;

            _aimPoint = aimPoint;
            _phaseTimer += deltaTime;

            while (_phase != AttackPhase.Done)
            {
                float duration = GetPhaseDuration();
                if (_phaseTimer < duration) break;

                _phaseTimer -= duration;
                AdvancePhase();
            }
        }

        public void CancelAttack()
        {
            if (!IsAttacking) return;

            if (weaponHitbox != null) weaponHitbox.Deactivate();
            _phase = AttackPhase.Done;
            _runtime = null;
            OnAttackEnded?.Invoke();
        }

        private bool IsAvailable(int index, float distance, bool hasLineOfSight)
        {
            EnemyAttackOption option = _data.attacks[index];
            if (option == null) return false;
            if (Time.time < _nextReadyTime[index]) return false;

            if (option.spell != null)
            {
                if (spellExecutor == null || castOrigin == null || !hasLineOfSight) return false;
            }
            else if (option.attack == null)
            {
                return false;
            }

            return distance >= option.minRange && distance <= ResolveMaxRange(option);
        }

        private float ResolveMaxRange(EnemyAttackOption option)
        {
            if (option.maxRange > 0f) return option.maxRange;
            if (option.spell != null) return 15f;

            float length = option.attack != null && option.attack.hitboxSize.z > 0f
                ? option.attack.hitboxSize.z
                : (Weapon != null ? Weapon.range : 1.5f);

            return length * 0.9f;
        }

        private float ResolveEngageDistance()
        {
            if (_data.engageDistance > 0f) return _data.engageDistance;
            if (_data.attacks == null || _data.attacks.Length == 0) return 1.5f;

            float shortest = float.MaxValue;
            foreach (EnemyAttackOption option in _data.attacks)
            {
                if (option == null) continue;
                shortest = Mathf.Min(shortest, ResolveMaxRange(option));
            }

            return shortest == float.MaxValue ? 1.5f : shortest * 0.9f;
        }

        private float GetPhaseDuration()
        {
            bool isSpell = _current.spell != null;

            switch (_phase)
            {
                case AttackPhase.Startup: return isSpell ? _current.spell.castTime : _current.attack.startupDuration;
                case AttackPhase.Active: return isSpell ? 0f : _current.attack.activeDuration;
                case AttackPhase.Recovery: return isSpell ? _current.spell.recoveryTime : _current.attack.recoveryDuration;
                default: return 0f;
            }
        }

        private void AdvancePhase()
        {
            switch (_phase)
            {
                case AttackPhase.Startup:
                    _phase = AttackPhase.Active;
                    BeginStrike();
                    break;

                case AttackPhase.Active:
                    EndStrike();
                    _phase = AttackPhase.Recovery;
                    break;

                case AttackPhase.Recovery:
                    _phase = AttackPhase.Done;
                    _runtime = null;
                    _nextAttackAllowedTime = Time.time + UnityEngine.Random.Range(_data.postAttackDelayMin, Mathf.Max(_data.postAttackDelayMin, _data.postAttackDelayMax));
                    OnAttackEnded?.Invoke();
                    break;
            }
        }

        private void BeginStrike()
        {
            if (_current.spell != null)
            {
                CastSpell();
            }
            else if (weaponHitbox != null)
            {
                weaponHitbox.Activate(_runtime);
            }

            OnStrike?.Invoke();
        }

        private void EndStrike()
        {
            if (_current.spell == null && weaponHitbox != null)
            {
                weaponHitbox.Deactivate();
            }
        }

        private void CastSpell()
        {
            Vector3 direction = _aimPoint - castOrigin.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                castOrigin.rotation = Quaternion.LookRotation(direction);
            }

            spellExecutor.Execute(_current.spell);
        }
    }
}
