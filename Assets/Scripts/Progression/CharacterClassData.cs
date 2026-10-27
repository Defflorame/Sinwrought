using UnityEngine;
using Game.Combat;
using Game.Stats;

namespace Game.Progression
{
    /// <summary>
    /// Класс персонажа (данные): стартовые характеристики, профиль сопротивлений, правила уровней
    /// и ветка способностей. Не хранит runtime state. Текущий класс держит ProgressionSystem.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Game/Progression/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {
        public string className;
        [TextArea] public string description;

        [Header("Start")]
        public BaseCharacterStats baseStats;
        public ResistanceProfileData resistanceProfile;

        [Header("Progression")]
        public LevelProgressionData levelProgression;

        [Header("Abilities")]
        public AbilityTreeData abilityTree;

        [Min(1)]
        [Tooltip("Очки способностей выдаются на каждом N-м уровне (5, 10, 15 ...).")]
        public int abilityPointInterval = 5;

        [Min(0)]
        [Tooltip("Сколько очков способностей даёт каждый такой уровень.")]
        public int abilityPointsPerInterval = 1;

        public int GetAbilityPointsForLevel(int level)
        {
            return level > 0 && level % abilityPointInterval == 0 ? abilityPointsPerInterval : 0;
        }
    }
}
