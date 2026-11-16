using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Единая точка «восстановить всё»: здоровье, выносливость, мана.
    /// Используется отладочным респавном, а позже — отдыхом у алтаря.
    /// Stateless. owner — корень сущности; отсутствующие компоненты пропускаются
    /// (например, у врага нет маны).
    /// Мёртвый владелец воскрешается, живой лечится до максимума.
    /// </summary>
    public static class VitalsRestoreService
    {
        public static void RestoreAll(GameObject owner)
        {
            if (owner == null) return;

            HealthComponent health = owner.GetComponentInChildren<HealthComponent>();
            if (health != null)
            {
                if (health.IsDead()) health.Revive();
                else health.Heal(health.MaxHealth);
            }

            StaminaComponent stamina = owner.GetComponentInChildren<StaminaComponent>();
            if (stamina != null) stamina.RestoreToFull();

            ManaComponent mana = owner.GetComponentInChildren<ManaComponent>();
            if (mana != null) mana.Restore(mana.MaxMana);
        }
    }
}