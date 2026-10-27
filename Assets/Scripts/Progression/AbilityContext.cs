using UnityEngine;

namespace Game.Progression
{
    /// <summary>Всё, что нужно эффекту способности: владелец, на котором он работает. Только данные.</summary>
    public readonly struct AbilityContext
    {
        public readonly GameObject Owner;

        public AbilityContext(GameObject owner)
        {
            Owner = owner;
        }

        /// <summary>Компонент владельца (например, CharacterStats). Ищет и в детях.</summary>
        public T GetOwnerComponent<T>() where T : class
        {
            return Owner != null ? Owner.GetComponentInChildren<T>() : null;
        }
    }
}
