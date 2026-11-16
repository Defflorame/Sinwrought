using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Game.World
{
    /// <summary>
    /// Стабильный Id объекта сцены (GUID) для сохранений. Генерируется в редакторе сам: при добавлении
    /// компонента и при копировании объекта (дубликат получает новый Id). В ассете префаба Id пустой,
    /// поэтому каждый экземпляр на сцене получает свой.
    /// </summary>
    public class PersistentId : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string id;

        public string Id => id;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Ассет префаба или режим редактирования префаба: Id не нужен.
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            bool inPrefabStage = stage != null && stage.scene == gameObject.scene;
            if (EditorUtility.IsPersistent(this) || inPrefabStage)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    id = "";
                    EditorUtility.SetDirty(this);
                }
                return;
            }

            if (!gameObject.scene.IsValid() || !gameObject.scene.isLoaded) return;
            if (string.IsNullOrEmpty(id) || IsDuplicate())
            {
                id = Guid.NewGuid().ToString("N");
                EditorUtility.SetDirty(this);
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
        }

        private bool IsDuplicate()
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                foreach (PersistentId other in root.GetComponentsInChildren<PersistentId>(true))
                {
                    if (other != this && other.id == id) return true;
                }
            }

            return false;
        }
#endif
    }
}