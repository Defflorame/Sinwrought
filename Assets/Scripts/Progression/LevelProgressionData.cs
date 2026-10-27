using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Stats;

namespace Game.Progression
{
    /// <summary>Сколько даёт одно очко конкретной характеристики. Статы, которых нет в списке, качать нельзя.</summary>
    [Serializable]
    public class StatPointRule
    {
        public StatType stat;

        [Tooltip("Сколько единиц характеристики (Flat) даёт одно очко.")]
        public float valuePerPoint = 1f;

        [Min(0)]
        [Tooltip("Максимум очков в эту характеристику. 0 — без ограничения.")]
        public int maxPoints = 0;
    }

    /// <summary>Ручная правка конкретного уровня поверх общей формулы. -1 в поле — взять общее правило.</summary>
    [Serializable]
    public class LevelOverride
    {
        [Min(2)]
        [Tooltip("Уровень, который игрок ДОСТИГАЕТ (2 — переход с 1 на 2).")]
        public int level = 2;

        [Tooltip("Сколько опыта нужно для перехода на этот уровень. -1 — по формуле.")]
        public int experienceCost = -1;

        [Tooltip("Сколько очков характеристик даёт этот уровень. -1 — общее значение.")]
        public int statPoints = -1;
    }

    /// <summary>
    /// Правила уровней: цена в опыте, очки характеристик за уровень, что и как качается.
    /// Цена перехода с уровня L: baseExperience × L^experienceExponent (степенная кривая, как в souls-играх:
    /// ранние уровни быстрые, поздние заметно дороже). Любой уровень можно подправить в overrides.
    /// Не хранит runtime state: текущий уровень знает ProgressionSystem.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelProgression", menuName = "Game/Progression/Level Progression Data")]
    public class LevelProgressionData : ScriptableObject
    {
        [Header("Levels")]
        [Min(1)] public int startLevel = 1;
        [Min(1)] public int maxLevel = 99;

        [Header("Experience cost")]
        [Min(1)] public int baseExperience = 25;
        [Min(0f)] public float experienceExponent = 1.6f;

        [Header("Points")]
        [Min(0)]
        [Tooltip("Сколько очков характеристик даёт каждый уровень (если не переопределено в overrides).")]
        public int statPointsPerLevel = 1;

        [SerializeField] private StatPointRule[] statRules;
        [SerializeField] private LevelOverride[] overrides;

        public IReadOnlyList<StatPointRule> StatRules => statRules ?? Array.Empty<StatPointRule>();

        public bool IsMaxLevel(int level) => level >= maxLevel;

        /// <summary>Опыт, нужный для перехода с currentLevel на следующий.</summary>
        public int GetExperienceForLevelUp(int currentLevel)
        {
            LevelOverride levelOverride = FindOverride(currentLevel + 1);
            if (levelOverride != null && levelOverride.experienceCost >= 0)
            {
                return levelOverride.experienceCost;
            }

            float cost = baseExperience * Mathf.Pow(Mathf.Max(1, currentLevel), experienceExponent);
            return Mathf.Max(1, Mathf.RoundToInt(cost));
        }

        /// <summary>Очки характеристик за достижение reachedLevel.</summary>
        public int GetStatPointsForLevel(int reachedLevel)
        {
            LevelOverride levelOverride = FindOverride(reachedLevel);
            if (levelOverride != null && levelOverride.statPoints >= 0)
            {
                return levelOverride.statPoints;
            }

            return statPointsPerLevel;
        }

        public StatPointRule FindStatRule(StatType stat)
        {
            if (statRules == null) return null;

            foreach (StatPointRule rule in statRules)
            {
                if (rule != null && rule.stat == stat) return rule;
            }

            return null;
        }

        private LevelOverride FindOverride(int level)
        {
            if (overrides == null) return null;

            foreach (LevelOverride levelOverride in overrides)
            {
                if (levelOverride != null && levelOverride.level == level) return levelOverride;
            }

            return null;
        }

        private void OnValidate()
        {
            maxLevel = Mathf.Max(startLevel, maxLevel);

            if (statRules == null) return;

            var seen = new HashSet<StatType>();
            foreach (StatPointRule rule in statRules)
            {
                if (rule != null && !seen.Add(rule.stat))
                {
                    Debug.LogWarning($"LevelProgressionData '{name}': характеристика {rule.stat} указана дважды — работает первая запись.", this);
                }
            }
        }
    }
}
