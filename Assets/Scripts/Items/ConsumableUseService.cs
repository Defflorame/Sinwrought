using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// Использование расходника: проверка, применение эффектов, снятие одного предмета из инвентаря.
    /// Stateless; вызывается из QuickSlotController, позже тем же способом — из UI инвентаря.
    /// </summary>
    public static class ConsumableUseService
    {
        /// <summary>Предмет есть в инвентаре и полезен хотя бы один его эффект.</summary>
        public static bool CanUse(ConsumableData data, GameObject owner, Inventory inventory)
        {
            if (data == null || owner == null || inventory == null) return false;
            if (!inventory.HasItem(data)) return false;

            var context = new ConsumableUseContext(owner);
            foreach (ConsumableEffectData effect in data.Effects)
            {
                if (effect != null && effect.CanUse(context)) return true;
            }

            return false;
        }

        /// <summary>Применяет полезные эффекты и снимает один предмет. false — ничего не произошло.</summary>
        public static bool TryUse(ConsumableData data, GameObject owner, Inventory inventory)
        {
            if (!CanUse(data, owner, inventory)) return false;

            ItemStack stack = inventory.FindItem(data);
            if (stack == null) return false;

            var context = new ConsumableUseContext(owner);
            foreach (ConsumableEffectData effect in data.Effects)
            {
                if (effect != null && effect.CanUse(context)) effect.Use(context);
            }

            inventory.RemoveItem(stack.Item);
            Debug.Log($"ConsumableUseService: использовано {data.name}");
            return true;
        }
    }
}