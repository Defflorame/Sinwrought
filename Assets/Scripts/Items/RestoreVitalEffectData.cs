using UnityEngine;
using Game.Combat;

namespace Game.Items
{
    /// <summary>Восстанавливает здоровье или ману владельца в процентах от максимума.</summary>
    [CreateAssetMenu(fileName = "NewRestoreVitalEffect", menuName = "Game/Items/Consumable Effects/Restore Vital")]
    public class RestoreVitalEffectData : ConsumableEffectData
    {
        public enum Vital
        {
            Health,
            Mana
        }

        [SerializeField] private Vital vital = Vital.Health;

        [Range(1f, 100f)]
        [Tooltip("Сколько восстановить, % от максимума.")]
        [SerializeField] private float percent = 40f;

        public override bool CanUse(ConsumableUseContext context)
        {
            switch (vital)
            {
                case Vital.Health:
                    HealthComponent health = context.GetOwnerComponent<HealthComponent>();
                    return health != null && !health.IsDead() && health.CurrentHealth < health.MaxHealth;

                case Vital.Mana:
                    ManaComponent mana = context.GetOwnerComponent<ManaComponent>();
                    return mana != null && mana.CurrentMana < mana.MaxMana;
            }

            return false;
        }

        public override void Use(ConsumableUseContext context)
        {
            switch (vital)
            {
                case Vital.Health:
                    HealthComponent health = context.GetOwnerComponent<HealthComponent>();
                    if (health != null)
                    {
                        // Минимум 1, чтобы малый процент не обнулялся при маленьком максимуме.
                        health.Heal(Mathf.Max(1, Mathf.RoundToInt(health.MaxHealth * percent / 100f)));
                    }
                    break;

                case Vital.Mana:
                    ManaComponent mana = context.GetOwnerComponent<ManaComponent>();
                    if (mana != null) mana.Restore(mana.MaxMana * percent / 100f);
                    break;
            }
        }
    }
}