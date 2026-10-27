using UnityEngine;

namespace Game.Progression
{
    /// <summary>Одна способность (узел ветки): описание и список эффектов. Не хранит, открыта ли она.</summary>
    [CreateAssetMenu(fileName = "NewAbilityNode", menuName = "Game/Progression/Ability Node Data")]
    public class AbilityNodeData : ScriptableObject
    {
        public string abilityName;
        [TextArea] public string description;
        public Sprite icon;

        [Tooltip("Цена в очках способностей. -1 — по правилу дерева (baseCost + ступень × costPerTier).")]
        public int costOverride = -1;

        public AbilityEffect[] effects;
    }
}
