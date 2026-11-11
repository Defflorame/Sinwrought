using System;
using UnityEngine;
using Game.Player.Stats;
using Game.Stats;

namespace Game.Combat
{
    /// <summary>
    /// Здоровье: получение урона, лечение, трата HP как ресурса, смерть.
    /// Если назначен CharacterStats — maxHealth берётся из характеристик и
    /// обновляется при их изменении (подписка на OnStatsChanged). Без него
    /// (манекены, враги) используется значение из инспектора.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        /// Если назначен CharacterStats — maxHealth берётся из характеристик и
        /// обновляется при их изменении (подписка на OnStatsChanged). Значение
        /// maxHealth из инспектора используется только без CharacterStats (манекены);
        /// у игрока и врагов оно перезаписывается в Start.
        [Tooltip("Только для объектов без CharacterStats (манекены). С CharacterStats игнорируется.")]
        [SerializeField] private int maxHealth = 100;
        [Tooltip("Необязательно. Если назначено — maxHealth задаётся характеристикой Strength.")]
        [SerializeField] private CharacterStats stats;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;

        public event Action<int, int> OnHealthChanged;
        public event Action<int> OnDamageTaken;
        public event Action OnDeath;

        private bool _isDead;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void OnEnable()
        {
            if (stats != null) stats.OnStatsChanged += HandleStatsChanged;
        }

        private void OnDisable()
        {
            if (stats != null) stats.OnStatsChanged -= HandleStatsChanged;
        }

        // Start, а не Awake: CharacterStats гарантированно посчитал Stats в своём Awake.
        private void Start()
        {
            if (stats != null && stats.Stats != null)
            {
                SetMaxHealth(stats.Stats.MaxHealth, refillToFull: true);
            }
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnDamageTaken?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"HealthComponent: {gameObject.name} получил {amount} урона, текущее хп {CurrentHealth}");

            if (CurrentHealth <= 0)
            {
                Kill();
            }
        }

        /// <summary>
        /// Трата HP как ресурса (заклинания за HP). Не считается уроном:
        /// нет OnDamageTaken, и никогда не убивает (минимум 1 HP).
        /// </summary>
        public void SpendHealth(int amount)
        {
            if (_isDead || amount <= 0) return;

            CurrentHealth = Mathf.Max(1, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void Heal(int amount)
        {
            if (_isDead || amount <= 0) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void Kill()
        {
            if (_isDead) return;

            _isDead = true;
            CurrentHealth = 0;
            OnDeath?.Invoke();
        }

        public bool IsDead() => _isDead;

        private void HandleStatsChanged(DerivedCharacterStats derived)
        {
            SetMaxHealth(derived.MaxHealth, refillToFull: false);
        }

        private void SetMaxHealth(int newMax, bool refillToFull)
        {
            maxHealth = Mathf.Max(1, newMax);
            CurrentHealth = refillToFull ? maxHealth : Mathf.Min(CurrentHealth, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
