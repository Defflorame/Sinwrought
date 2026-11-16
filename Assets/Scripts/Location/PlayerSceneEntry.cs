using UnityEngine;
using Game.Player;

namespace Game.Locations
{
    /// <summary>
    /// Вход игрока в сцену: если игрока ещё нет — создаёт из префаба и делает постоянным,
    /// затем ставит на точку появления и подключает к врагам сцены.
    /// Единственное место создания игрока (так же пойдут «Новая игра» и «Загрузить»).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class PlayerSceneEntry : MonoBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private LocationRoot locationRoot;

        private void Awake()
        {
            PlayerController player = GameSession.Player;
            if (player == null)
            {
                player = Instantiate(playerPrefab);
                DontDestroyOnLoad(player.gameObject);
                GameSession.RegisterPlayer(player);
            }

            SpawnPoint spawn = ResolveSpawnPoint();
            if (spawn == null)
            {
                Debug.LogError("PlayerSceneEntry: в LocationRoot не назначена точка по умолчанию.", this);
                return;
            }

            player.TeleportTo(spawn.transform.position, spawn.transform.rotation);
            locationRoot.InitializeEnemies(player);
        }

        private SpawnPoint ResolveSpawnPoint()
        {
            SpawnPointData requested = GameSession.TakePendingSpawn();
            if (requested == null) return locationRoot.DefaultSpawnPoint;

            if (locationRoot.TryGetSpawnPoint(requested, out SpawnPoint point)) return point;

            Debug.LogWarning($"PlayerSceneEntry: в сцене нет точки {requested.name}, используется точка по умолчанию.", this);
            return locationRoot.DefaultSpawnPoint;
        }
    }
}