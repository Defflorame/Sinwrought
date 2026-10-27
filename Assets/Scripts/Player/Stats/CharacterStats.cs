using System;
using System.Collections.Generic;
using Game.Player.Stats;
using UnityEngine;

namespace Game.Stats
{
    /// <summary>
    /// Хранит базовые характеристики (BaseCharacterStats) и активные StatModifier,
    /// пересчитывает DerivedCharacterStats и сообщает об изменении событием.
    /// Ничего не знает о Combat: HealthComponent/StaminaComponent/ManaComponent
    /// сами подписываются на OnStatsChanged (зависимость Combat -> Stats, а не наоборот).
    /// PhysicalDamageBonusPercent и DefenseValue читает DamageResolver.
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [SerializeField] private BaseCharacterStats baseStats;

        public DerivedCharacterStats Stats { get; private set; }

        public event Action<DerivedCharacterStats> OnStatsChanged;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        private void Awake()
        {
            Recalculate();
        }

        /// <summary>Подменяет базовые характеристики (стартовые значения выбранного класса). Модификаторы сохраняются.</summary>
        public void SetBaseStats(BaseCharacterStats newBaseStats)
        {
            if (newBaseStats == null) return;

            baseStats = newBaseStats;
            Recalculate();
            OnStatsChanged?.Invoke(Stats);
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            Recalculate();
            OnStatsChanged?.Invoke(Stats);
        }

        /// <summary>Снимает все модификаторы, добавленные конкретным источником (например, снятым кольцом).</summary>
        public void RemoveModifiersFromSource(object source)
        {
            int removed = _modifiers.RemoveAll(m => m.Source == source);
            if (removed == 0) return;

            Recalculate();
            OnStatsChanged?.Invoke(Stats);
        }

        private void Recalculate()
        {
            if (baseStats == null)
            {
                Debug.LogWarning("CharacterStats: BaseCharacterStats не назначен.", this);
                Stats = new DerivedCharacterStats(1, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
                return;
            }

            float strength = ApplyModifiers(StatType.Strength, baseStats.strength);
            int maxHealth = baseStats.baseHealth + Mathf.RoundToInt(strength * baseStats.healthPerStrengthPoint);
            // Flat — процентные пункты к бонусу (вместе с бонусом от силы).
            // Percent — отдельный множитель на весь физический урон.
            float flatBonus = strength * baseStats.physicalDamageBonusPerStrengthPoint
                + SumModifiers(StatType.PhysicalDamageBonus, StatModifierType.Flat);
            float damageMultiplier = (1f + flatBonus / 100f)
                * (1f + SumModifiers(StatType.PhysicalDamageBonus, StatModifierType.Percent) / 100f);
            float physicalDamageBonusPercent = (damageMultiplier - 1f) * 100f;

            float maxStamina = ApplyModifiers(StatType.Stamina, baseStats.stamina);
            float staminaRegen = ApplyModifiers(StatType.StaminaRegen, baseStats.staminaRegenPerSecond);
            float staminaCostMultiplier = ApplyModifiers(StatType.StaminaCost, 1f);
            float castSpeedMultiplier = ApplyModifiers(StatType.CastSpeed, 1f);
            float maxMana = ApplyModifiers(StatType.Mana, baseStats.mana);
            float magicDamage = ApplyModifiers(StatType.MagicDamage, baseStats.magicDamage);
            float speed = ApplyModifiers(StatType.Speed, baseStats.speed);
            float carryWeight = ApplyModifiers(StatType.CarryWeight, baseStats.carryWeight);

            float defenseStat = ApplyModifiers(StatType.Defense, baseStats.defense);
            float defenseValue = defenseStat * baseStats.damageReductionPerDefensePoint;

            Stats = new DerivedCharacterStats(
                maxHealth: Mathf.Max(1, maxHealth),
                maxStamina: Mathf.Max(0f, maxStamina),
                staminaRegenPerSecond: Mathf.Max(0f, staminaRegen),
                physicalDamageBonusPercent: Mathf.Max(-100f, physicalDamageBonusPercent),
                maxMana: Mathf.Max(0f, maxMana),
                magicDamage: magicDamage,
                defenseValue: Mathf.Max(0f, defenseValue),
                movementSpeed: Mathf.Max(0f, speed),
                carryWeightLimit: Mathf.Max(0f, carryWeight),
                // Нижняя граница 0.1: иначе большие бонусы сделают действия бесплатными или каст бесконечно быстрым/медленным.
                staminaCostMultiplier: Mathf.Max(0.1f, staminaCostMultiplier),
                castSpeedMultiplier: Mathf.Max(0.1f, castSpeedMultiplier));
            Debug.Log($"CharacterStats: recalculated -> HP {Stats.MaxHealth}, Strength {strength:F1}, Stamina {Stats.MaxStamina:F1} (regen {Stats.StaminaRegenPerSecond:F1}/s, cost x{Stats.StaminaCostMultiplier:F2}), " +
      $"Mana {Stats.MaxMana:F1}, PhysicalDamageBonus {Stats.PhysicalDamageBonusPercent:F1}%, MagicDamage {Stats.MagicDamage:F1}, CastSpeed x{Stats.CastSpeedMultiplier:F2}, " +
      $"Defense {Stats.DefenseValue:F1}, Speed {Stats.MovementSpeed:F1}, CarryWeight {Stats.CarryWeightLimit:F1}");
        }

        /// <summary>(base + сумма Flat) * (1 + сумма Percent / 100). Общая формула для всех характеристик.</summary>
        private float ApplyModifiers(StatType stat, float baseValue)
        {
            float flatSum = 0f;
            float percentSum = 0f;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];
                if (modifier.Stat != stat) continue;

                if (modifier.Type == StatModifierType.Flat)
                {
                    flatSum += modifier.Value;
                }
                else
                {
                    percentSum += modifier.Value;
                }
            }

            return (baseValue + flatSum) * (1f + percentSum / 100f);
        }

        private float SumModifiers(StatType stat, StatModifierType type)
        {
            float sum = 0f;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];
                if (modifier.Stat == stat && modifier.Type == type) sum += modifier.Value;
            }
            return sum;
        }
    }
}
