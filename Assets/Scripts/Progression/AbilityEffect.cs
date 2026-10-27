using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// База для эффекта способности. Новый эффект — новый наследник + ассет;
    /// AbilityTreeController не меняется. Эффект не хранит runtime state.
    /// source — узел дерева, открывший способность: по нему эффект снимает именно свои изменения.
    /// </summary>
    public abstract class AbilityEffect : ScriptableObject
    {
        public abstract void Apply(AbilityContext context, object source);

        public abstract void Remove(AbilityContext context, object source);
    }
}
