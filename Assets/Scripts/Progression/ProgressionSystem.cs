using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Combat;
using Game.Stats;

namespace Game.Progression
{
    /// <summary>
    /// Уровень игрока. Опыт из ProgressionResourceWallet автоматически превращается в уровни
    /// (опыт при этом тратится по цене из LevelProgressionData); каждый уровень даёт очки характеристик.
    /// Очки можно потратить только пока открыт PointSpendingGate (у кристалла).
    /// Очки способностей выдаёт AbilityPointController, слушая OnLevelChanged.
    /// Также применяет к игроку стартовые данные текущего класса.
    /// </summary>
    public class ProgressionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterClassData characterClass;
        [SerializeField] private ProgressionResourceWallet resourceWallet;
        [SerializeField] private PointSpendingGate gate;
        [SerializeField] private CharacterStats characterStats;
        [Tooltip("Необязательно. Если назначен — получает профиль сопротивлений класса.")]
        [SerializeField] private DamageReceiver damageReceiver;

        private readonly Dictionary<StatType, int> _allocated = new Dictionary<StatType, int>();
        private bool _isLevelingUp;

        public CharacterClassData CurrentClass => characterClass;
        public int Level { get; private set; }
        public int UnspentStatPoints { get; private set; }

        private LevelProgressionData Rules => characterClass != null ? characterClass.levelProgression : null;

        public bool IsMaxLevel => Rules == null || Rules.IsMaxLevel(Level);

        /// <summary>Опыт, нужный для следующего уровня. 0 на максимальном уровне.</summary>
        public int ExperienceToNextLevel => IsMaxLevel ? 0 : Rules.GetExperienceForLevelUp(Level);

        /// <summary>Вызывается на КАЖДЫЙ набранный уровень (при получении сразу нескольких — по разу на каждый).</summary>
        public event Action<int> OnLevelChanged;

        /// <summary>Изменилось число нераспределённых очков характеристик или их распределение.</summary>
        public event Action OnStatPointsChanged;

        private void Awake()
        {
            if (characterClass == null)
            {
                Debug.LogWarning("ProgressionSystem: не назначен characterClass.", this);
                Level = 1;
                return;
            }

            Level = Rules != null ? Rules.startLevel : 1;
            ApplyClass();
        }

        private void OnEnable()
        {
            resourceWallet.OnChanged += HandleWalletChanged;
        }

        private void OnDisable()
        {
            resourceWallet.OnChanged -= HandleWalletChanged;
        }

        public int GetAllocatedPoints(StatType stat)
        {
            return _allocated.TryGetValue(stat, out int points) ? points : 0;
        }

        public bool CanAllocateStatPoint(StatType stat)
        {
            if (gate == null || !gate.IsOpen || UnspentStatPoints <= 0 || Rules == null) return false;

            StatPointRule rule = Rules.FindStatRule(stat);
            if (rule == null) return false;

            return rule.maxPoints == 0 || GetAllocatedPoints(stat) < rule.maxPoints;
        }

        public bool TryAllocateStatPoint(StatType stat)
        {
            if (!CanAllocateStatPoint(stat)) return false;

            StatPointRule rule = Rules.FindStatRule(stat);

            UnspentStatPoints--;
            _allocated[stat] = GetAllocatedPoints(stat) + 1;
            characterStats.AddModifier(new StatModifier(stat, StatModifierType.Flat, rule.valuePerPoint, this));

            OnStatPointsChanged?.Invoke();
            return true;
        }

        private void ApplyClass()
        {
            if (characterClass.baseStats != null)
            {
                characterStats.SetBaseStats(characterClass.baseStats);
            }

            if (characterClass.resistanceProfile != null && damageReceiver != null)
            {
                damageReceiver.SetResistanceProfile(characterClass.resistanceProfile);
            }
        }

        private void HandleWalletChanged(int amount, int delta)
        {
            if (delta > 0) ProcessLevelUps();
        }

        private void ProcessLevelUps()
        {
            // Трата опыта сама вызывает OnChanged кошелька — защита от повторного входа.
            if (_isLevelingUp || Rules == null) return;

            _isLevelingUp = true;

            while (!IsMaxLevel)
            {
                if (!resourceWallet.TrySpend(Rules.GetExperienceForLevelUp(Level))) break;

                Level++;
                UnspentStatPoints += Rules.GetStatPointsForLevel(Level);

                Debug.Log($"ProgressionSystem: уровень {Level}, очков характеристик {UnspentStatPoints}");
                OnLevelChanged?.Invoke(Level);
                OnStatPointsChanged?.Invoke();
            }

            _isLevelingUp = false;
        }
    }
}
