using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// База для условия прохода (ключ, аксессуар и т.д.). Новое условие — новый наследник + ассет;
    /// WorldGate и GasBarrier не меняются. owner — корень игрока, компоненты ищутся через GetComponentInChildren.
    /// </summary>
    public abstract class AccessRequirement : ScriptableObject
    {
        public abstract bool IsMet(GameObject owner);

        /// <summary>Что забрать у владельца при использовании условия (ключ). По умолчанию ничего.</summary>
        public virtual void Consume(GameObject owner) { }

        /// <summary>Все ли условия выполнены. Пустой список = условий нет. Пустая запись в списке = не выполнено.</summary>
        public static bool AreAllMet(AccessRequirement[] requirements, GameObject owner)
        {
            if (requirements == null) return true;

            foreach (AccessRequirement requirement in requirements)
            {
                if (requirement == null || !requirement.IsMet(owner)) return false;
            }

            return true;
        }
    }
}