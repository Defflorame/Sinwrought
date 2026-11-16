using UnityEngine;
using Game.Save;

namespace Game.Progression
{
    /// <summary>Каталог "Id → класс персонажа" для сохранений.</summary>
    [CreateAssetMenu(fileName = "CharacterClassCatalog", menuName = "Game/Progression/Character Class Catalog Data")]
    public class CharacterClassCatalogData : CatalogData<CharacterClassData>
    {
        protected override string GetId(CharacterClassData item) => item.Id;

#if UNITY_EDITOR
        [ContextMenu("Rebuild")]
        private void Rebuild() => RebuildEntries();
#endif
    }
}