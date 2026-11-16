using UnityEngine;
using Game.Locations;
using Game.World;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Altars
{
    /// <summary>
    /// Описание алтаря (данные, не runtime state). Id выводится из GUID ассета
    /// и стабилен при переименовании и перемещении (понадобится сохранению в Phase 11).
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Altars/Altar Data")]
    public class AltarData : ScriptableObject
    {
        [SerializeField, HideInInspector] private string id;

        [SerializeField] private string altarName;
        [Tooltip("Локация, в которой стоит алтарь.")]
        [SerializeField] private LocationData location;
        [Tooltip("Точка появления игрока у алтаря (в сцене локации должен стоять SpawnPoint с этими данными).")]
        [SerializeField] private SpawnPointData spawnPoint;
        [Tooltip("Условия, при которых алтарь становится доступен для fast travel (используются в 10.3). Пусто — условий нет.")]
        [SerializeField] private AccessRequirement[] fastTravelRequirements;

        public string Id => id;
        public string AltarName => string.IsNullOrEmpty(altarName) ? name : altarName;
        public LocationData Location => location;
        public SpawnPointData SpawnPoint => spawnPoint;
        public AccessRequirement[] FastTravelRequirements => fastTravelRequirements;

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