using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

namespace Game.Save
{
    /// <summary>
    /// Каталог "Id → ассет" для сохранений (ни строк-путей, ни Resources). Заполняется в редакторе:
    /// ПКМ по заголовку компонента в Inspector → Rebuild. Наследник объявляет, где у ассета Id.
    /// </summary>
    public abstract class CatalogData<T> : ScriptableObject where T : ScriptableObject
    {
        [SerializeField] private T[] entries;

        protected abstract string GetId(T item);

        public bool TryGet(string id, out T item)
        {
            item = null;
            if (string.IsNullOrEmpty(id) || entries == null) return false;

            foreach (T entry in entries)
            {
                if (entry != null && GetId(entry) == id)
                {
                    item = entry;
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        protected void RebuildEntries()
        {
            var found = new List<T>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null) continue;

                if (string.IsNullOrEmpty(GetId(asset)))
                {
                    Debug.LogWarning($"{asset.name}: пустой Id, сохранение не сможет на него сослаться.", asset);
                }

                found.Add(asset);
            }

            Undo.RecordObject(this, "Rebuild catalog");
            entries = found.ToArray();
            EditorUtility.SetDirty(this);
            Debug.Log($"{name}: каталог пересобран, записей: {entries.Length}", this);
        }
#endif
    }
}