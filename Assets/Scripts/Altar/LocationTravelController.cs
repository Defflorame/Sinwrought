// Locations/LocationTravelController.cs
using UnityEngine;
using Game.Player;

namespace Game.Locations
{
    /// <summary>
    /// «Перейти в локацию на точку». Та же локация — телепорт без перезагрузки сцены
    /// (состояние дверей не сбрасывается), другая — загрузка через LocationLoader.
    /// Не знает, кто и зачем запросил переход (алтарь, портал и т.д.).
    /// </summary>
    public class LocationTravelController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LocationLoader loader;

        /// <summary>false — переход не начат.</summary>
        public bool TryTravel(LocationData location, SpawnPointData spawnPoint)
        {
            if (location == null || spawnPoint == null || loader.IsLoading) return false;

            LocationRoot current = GameSession.CurrentLocationRoot;
            if (current != null && current.Location == location)
            {
                if (!current.TryGetSpawnPoint(spawnPoint, out SpawnPoint point))
                {
                    Debug.LogWarning($"LocationTravelController: в текущей сцене нет точки {spawnPoint.name}.", this);
                    return false;
                }

                playerController.TeleportTo(point.transform.position, point.transform.rotation);
                return true;
            }

            return loader.TryLoad(location, spawnPoint);
        }
    }
}