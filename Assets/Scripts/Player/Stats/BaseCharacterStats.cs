using UnityEngine;

namespace Game.Stats
{
    /// <summary>
    /// Описывает базовые характеристики персонажа. Health и Mana больше
    /// не самостоятельные инвестируемые характеристики: Health — производная
    /// от Strength (сила даёт и бонус физ. урона, и живучесть), Mana теперь
    /// инвестируется напрямую (без промежуточного Intelligence, по аналогии
    /// с тем, как уже устроена Stamina). Defense — боевая характеристика,
    /// снижающая входящий урон. Не хранит runtime
    /// состояние конкретного прохождения — этим занимаются
    /// CharacterStats/DerivedCharacterStats.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBaseCharacterStats", menuName = "Game/Player/Base Character Stats")]
    public class BaseCharacterStats : ScriptableObject
    {
        [Header("Strength")]
        public float strength = 10f;

        [Tooltip("Здоровье персонажа без учёта Strength (базовая живучесть на 0 очков силы).")]
        public int baseHealth = 50;

        [Tooltip("Сколько единиц MaxHealth даёт одно очко Strength.")]
        public float healthPerStrengthPoint = 5f;

        [Tooltip("Сколько процентов бонуса к физическому урону даёт одно очко Strength.")]
        public float physicalDamageBonusPerStrengthPoint = 1f;

        [Header("Stamina")]
        [Tooltip("Определяет максимальный запас выносливости. Скорость регенерации — фиксированная константа.")]
        public float stamina = 100f;
        public float staminaRegenPerSecond = 15f;

        [Header("Mana")]
        [Tooltip("Запас маны.")]
        public float mana = 50f;

        [Header("Combat")]
        public float magicDamage = 5f;

        [Header("Defense")]
        public float defense = 0f;
        [Tooltip("Сколько единиц входящего урона снимает одно очко Defense.")]
        public float damageReductionPerDefensePoint = 1f;

        [Header("Movement")]
        public float speed = 5f;

        [Header("Encumbrance")]
        [Tooltip("Максимальный вес экипировки, который персонаж может нести без штрафов.")]
        public float carryWeight = 40f;

        [Header("Stagger")]
        [Min(0f)]
        [Tooltip("Запас устойчивости к оглушению (poise). Каждый удар отнимает staggerPower атаки; когда запас кончился — персонаж оглушён. Броня, кольца и способности увеличивают его через StatType.Poise. Ориентир: лёгкая атака ≈ 10, тяжёлая 30–40, двуручная 50+. 0 — оглушает любой удар.")]
        public float poise = 20f;
    }
}