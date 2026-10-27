using System;
using UnityEngine;
using Game.Player.Stats;
using Game.Stats;

namespace Game.Combat
{
    /// <summary>
    /// Выносливость. Если назначен CharacterStats — максимум, скорость регенерации и множитель
    /// трат берутся из характеристик (подписка на OnStatsChanged).
    /// Без него используются значения из инспектора.
    /// Итоговый множитель трат = нагрузка экипировки (SetCostMultiplier) × характеристика StaminaCost.
    /// </summary>
    public class StaminaComponent : MonoBehaviour
    {
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float regenPerSecond = 15f;
        [Tooltip("Пауза перед стартом регенерации после траты стамины.")]
        [SerializeField] private float regenDelayAfterConsume = 0.5f;
        [Tooltip("Необязательно. Если назначено — max и regen задаются характеристиками.")]
        [SerializeField] private CharacterStats stats;

        public float CurrentStamina { get; private set; }

        public event Action<float, float> OnStaminaChanged;

        private bool _regenEnabled = true;
        private float _regenBlockedUntil;
        private float _loadCostMultiplier = 1f;
        private float _statsCostMultiplier = 1f;

        public float CostMultiplier => _loadCostMultiplier * _statsCostMultiplier;


        private void Awake()
        {
            CurrentStamina = maxStamina;
        }

        private void OnEnable()
        {
            if (stats != null) stats.OnStatsChanged += HandleStatsChanged;
        }

        private void OnDisable()
        {
            if (stats != null) stats.OnStatsChanged -= HandleStatsChanged;
        }

        private void Start()
        {
            if (stats != null && stats.Stats != null)
            {
                ApplyStats(stats.Stats, refillToFull: true);
            }
        }

        private void Update()
        {
            if (!_regenEnabled) return;
            if (Time.time < _regenBlockedUntil) return;
            if (CurrentStamina >= maxStamina) return;

            Restore(regenPerSecond * Time.deltaTime);
        }

        /// <summary>baseCost — базовая цена; множитель нагрузки применяется внутри.</summary>
        public bool CanSpend(float baseCost)
        {
            return CurrentStamina >= baseCost * CostMultiplier;
        }

        public bool TryConsume(float baseCost)
        {
            if (baseCost <= 0f) return true;

            float cost = baseCost * CostMultiplier;
            if (CurrentStamina < cost)
            {
                Debug.Log($"StaminaComponent: недостаточно stamina ({CurrentStamina:F1}/{cost:F1})");
                return false;
            }

            CurrentStamina -= cost;
            _regenBlockedUntil = Time.time + regenDelayAfterConsume;
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
            return true;
        }

        public void Restore(float amount)
        {
            if (amount <= 0f) return;

            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + amount);
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }

        public void StartRegeneration() => _regenEnabled = true;

        public void StopRegeneration() => _regenEnabled = false;

        private void HandleStatsChanged(DerivedCharacterStats derived)
        {
            ApplyStats(derived, refillToFull: false);
        }

        private void ApplyStats(DerivedCharacterStats derived, bool refillToFull)
        {
            maxStamina = Mathf.Max(0f, derived.MaxStamina);
            regenPerSecond = Mathf.Max(0f, derived.StaminaRegenPerSecond);
            _statsCostMultiplier = Mathf.Max(0f, derived.StaminaCostMultiplier);
            CurrentStamina = refillToFull ? maxStamina : Mathf.Min(CurrentStamina, maxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }

        /// <summary>Множитель от нагрузки экипировки (EncumbranceController). Множитель от характеристик хранится отдельно.</summary>
        public void SetCostMultiplier(float multiplier)
        {
            _loadCostMultiplier = Mathf.Max(0f, multiplier);
        }
    }
}
