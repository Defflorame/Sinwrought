using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Progression
{
    /// <summary>Одна способность (узел ветки): описание и список эффектов. Не хранит, открыта ли она.</summary>
    [CreateAssetMenu(fileName = "NewAbilityNode", menuName = "Game/Progression/Ability Node Data")]
    public class AbilityNodeData : ScriptableObject
    {
        [SerializeField, HideInInspector] private string id;

        /// <summary>Стабильный Id для сохранений (GUID ассета).</summary>
        public string Id => id;
        public string abilityName;
        [TextArea] public string description;
        public Sprite icon;

        [Tooltip("Цена в очках способностей. -1 — по правилу дерева (baseCost + ступень × costPerTier).")]
        public int costOverride = -1;

        public AbilityEffect[] effects;
#if UNITY_EDITOR
        private void OnValidate()
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
            if (!string.IsNullOrEmpty(guid) && id != guid)
            {
                id = guid;
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
