using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Locations
{
    /// <summary>
    /// Идентификатор точки появления игрока (одна из дверей локации и т.п.).
    /// Id выводится из GUID ассета и стабилен при переименовании и перемещении.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Locations/Spawn Point Data")]
    public class SpawnPointData : ScriptableObject
    {
        [SerializeField, HideInInspector] private string id;

        public string Id => id;

#if UNITY_EDITOR
        private void OnValidate()
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
            if (id == guid) return;

            id = guid;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}