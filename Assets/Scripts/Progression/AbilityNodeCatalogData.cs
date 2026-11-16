using UnityEngine;
using Game.Save;

namespace Game.Progression
{
    /// <summary>Каталог "Id → узел способности" для сохранений.</summary>
    [CreateAssetMenu(fileName = "AbilityNodeCatalog", menuName = "Game/Progression/Ability Node Catalog Data")]
    public class AbilityNodeCatalogData : CatalogData<AbilityNodeData>
    {
        protected override string GetId(AbilityNodeData item) => item.Id;

#if UNITY_EDITOR
        [ContextMenu("Rebuild")]
        private void Rebuild() => RebuildEntries();
#endif
    }
}