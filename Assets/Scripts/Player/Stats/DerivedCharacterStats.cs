namespace Game.Player.Stats
{
    /// <summary>
    /// Итоговые характеристики персонажа после применения всех StatModifier
    /// к значениям BaseCharacterStats. Пересчитывается CharacterStats целиком
    /// при любом изменении — не мутируется частично, поэтому иммутабельна.
    ///
    /// PhysicalDamageBonusPercent и DefenseValue — боевые модификаторы без
    /// своего компонента-пула: их не проталкивают ни в один Component,
    /// их читает напрямую Combat-система (DamageResolver) в момент расчёта
    /// урона. StaminaCostMultiplier читает StaminaComponent, CastSpeedMultiplier — SpellCaster.
    /// </summary>
    public class DerivedCharacterStats
    {
        public int MaxHealth { get; }
        public float MaxStamina { get; }
        public float StaminaRegenPerSecond { get; }
        public float PhysicalDamageBonusPercent { get; }
        public float MaxMana { get; }
        public float MagicDamage { get; }
        public float DefenseValue { get; }
        public float MovementSpeed { get; }
        public float CarryWeightLimit { get; }
        public float StaminaCostMultiplier { get; }
        public float CastSpeedMultiplier { get; }

        public DerivedCharacterStats(
            int maxHealth,
            float maxStamina,
            float staminaRegenPerSecond,
            float physicalDamageBonusPercent,
            float maxMana,
            float magicDamage,
            float defenseValue,
            float movementSpeed,
            float carryWeightLimit,
            float staminaCostMultiplier = 1f,
            float castSpeedMultiplier = 1f)
        {
            MaxHealth = maxHealth;
            MaxStamina = maxStamina;
            StaminaRegenPerSecond = staminaRegenPerSecond;
            PhysicalDamageBonusPercent = physicalDamageBonusPercent;
            MaxMana = maxMana;
            MagicDamage = magicDamage;
            DefenseValue = defenseValue;
            MovementSpeed = movementSpeed;
            CarryWeightLimit = carryWeightLimit;
            StaminaCostMultiplier = staminaCostMultiplier;
            CastSpeedMultiplier = castSpeedMultiplier;
        }
    }
}
