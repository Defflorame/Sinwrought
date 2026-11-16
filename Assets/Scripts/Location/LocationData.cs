using UnityEngine;

namespace Game.Locations
{
    /// <summary>Описание локации. Единственное место со строкой: SO не может ссылаться на сцену напрямую.</summary>
    [CreateAssetMenu(menuName = "Game/Locations/Location Data")]
    public class LocationData : ScriptableObject
    {
        [Tooltip("Точное имя файла сцены. Сцена должна быть в Build Settings.")]
        [SerializeField] private string sceneName;

        public string SceneName => sceneName;
    }
}