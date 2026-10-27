using UnityEngine;
using Game.Items;
using Game.Stats;

namespace Game.Progression
{
    /// <summary>
    /// Пассивная способность через модификаторы характеристик — тот же путь, что у экипировки
    /// и статус-эффектов. Для способностей предназначены новые характеристики:
    /// StaminaCost (Percent −15 → траты стамины ×0.85), StaminaRegen (Percent +25 → регенерация ×1.25),
    /// CastSpeed (Percent +20 → каст и восстановление после него на 20% быстрее).
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatAbilityEffect", menuName = "Game/Progression/Effects/Stat Modifiers")]
    public class StatModifierAbilityEffect : AbilityEffect
    {
        [SerializeField] private StatModifierDefinition[] modifiers;

        public override void Apply(AbilityContext context, object source)
        {
            CharacterStats stats = context.GetOwnerComponent<CharacterStats>();
            if (stats == null)
            {
                Debug.LogWarning($"StatModifierAbilityEffect '{name}': у владельца нет CharacterStats.", this);
                return;
            }

            if (modifiers == null) return;

            foreach (StatModifierDefinition definition in modifiers)
            {
                stats.AddModifier(new StatModifier(definition.stat, definition.type, definition.value, source));
            }
        }

        public override void Remove(AbilityContext context, object source)
        {
            CharacterStats stats = context.GetOwnerComponent<CharacterStats>();
            if (stats != null) stats.RemoveModifiersFromSource(source);
        }
    }
}
