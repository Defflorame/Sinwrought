using UnityEngine;
using Game.World;

namespace Game.Locations
{
    /// <summary>
    /// Переход в другую локацию. Загрузчик берётся у interactor (корень игрока),
    /// ссылкой из сцены на него не сослаться.
    /// </summary>
    public class LocationPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private LocationData destination;
        [Tooltip("Точка в целевой сцене, на которой появится игрок.")]
        [SerializeField] private SpawnPointData destinationSpawnPoint;
        [SerializeField] private string prompt = "Войти";

        private LocationLoader _loader;

        public bool CanInteract(GameObject interactor)
        {
            if (destination == null || destinationSpawnPoint == null) return false;

            LocationLoader loader = GetLoader(interactor);
            return loader != null && !loader.IsLoading;
        }

        public void Interact(GameObject interactor)
        {
            LocationLoader loader = GetLoader(interactor);
            if (loader != null) loader.TryLoad(destination, destinationSpawnPoint);
        }

        public string GetInteractionPrompt() => prompt;

        // Кэш: CanInteract вызывается каждый кадр, пока игрок смотрит на портал.
        private LocationLoader GetLoader(GameObject interactor)
        {
            if (_loader == null) _loader = interactor.GetComponent<LocationLoader>();
            return _loader;
        }
    }
}