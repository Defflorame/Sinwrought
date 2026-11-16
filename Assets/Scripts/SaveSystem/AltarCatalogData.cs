// Game.Altars/AltarCatalogData.cs
using UnityEngine;
using Game.Save;

namespace Game.Altars
{
    [CreateAssetMenu(menuName = "Game/Altars/Altar Catalog Data")]
    public class AltarCatalogData : CatalogData<AltarData>
    {
        protected override string GetId(AltarData item) => item.Id;

#if UNITY_EDITOR
        [ContextMenu("Rebuild")]
        private void Rebuild() => RebuildEntries();
#endif
    }
}