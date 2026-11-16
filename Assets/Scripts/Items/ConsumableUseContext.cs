using UnityEngine;

namespace Game.Items
{
    /// <summary>Всё, что нужно эффекту расходника: владелец (корень игрока). Только данные.</summary>
    public readonly struct ConsumableUseContext
    {
        public readonly GameObject Owner;

        public ConsumableUseContext(GameObject owner)
        {
            Owner = owner;
        }

        /// <summary>Компонент владельца (например, HealthComponent). Ищет и в детях.</summary>
        public T GetOwnerComponent<T>() where T : class
        {
            return Owner != null ? Owner.GetComponentInChildren<T>() : null;
        }
    }
}