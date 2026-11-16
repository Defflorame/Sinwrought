using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// База для эффекта расходника. Новый эффект — новый наследник + ассет; ConsumableUseService
    /// и QuickSlotController не меняются. Эффект не хранит runtime state.
    /// </summary>
    public abstract class ConsumableEffectData : ScriptableObject
    {
        /// <summary>Есть ли сейчас смысл применять эффект (например, зелье не нужно при полном HP).</summary>
        public abstract bool CanUse(ConsumableUseContext context);

        public abstract void Use(ConsumableUseContext context);
    }
}