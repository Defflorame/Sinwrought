using UnityEngine;
using Game.Save;

namespace Game.Items
{
    /// <summary>Каталог «Id → предмет» для сохранений. Один на все типы предметов.</summary>
    [CreateAssetMenu(menuName = "Game/Items/Item Catalog Data")]
    public class ItemCatalogData : CatalogData<ItemData>
    {
        // Id = 0 значит «ещё не выведен из GUID»: такой предмет в каталог не попадёт (будет предупреждение).
        protected override string GetId(ItemData item) => item.Id == 0 ? "" : item.Id.ToString();

        public bool TryGet(int id, out ItemData item) => TryGet(id.ToString(), out item);

#if UNITY_EDITOR
        [ContextMenu("Rebuild")]
        private void Rebuild() => RebuildEntries();
#endif
    }
}