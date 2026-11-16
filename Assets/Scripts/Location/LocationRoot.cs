using System.Collections.Generic;
using UnityEngine;
using Game.Enemies;
using Game.Player;
using Game.Progression;

namespace Game.Locations
{
    /// <summary>
    /// Корень локации: знает точки появления сцены и связывает врагов с игроком.
    /// Собирает объекты один раз при загрузке сцены (не в игровой логике).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class LocationRoot : MonoBehaviour
    {
        [Tooltip("Данные этой локации (нужны fast travel, чтобы отличить «та же сцена» от другой).")]
        [SerializeField] private LocationData location;

        [Tooltip("Точка, на которую встаёт игрок, если вход не указал другую (запуск кнопкой Play).")]
        [SerializeField] private SpawnPoint defaultSpawnPoint;

        public LocationData Location => location;

        private readonly Dictionary<SpawnPointData, SpawnPoint> _spawnPoints = new Dictionary<SpawnPointData, SpawnPoint>();

        public SpawnPoint DefaultSpawnPoint => defaultSpawnPoint;

        private void Awake()
        {
            GameSession.RegisterLocation(this);
            foreach (SpawnPoint point in CollectInScene<SpawnPoint>())
            {
                if (point.Data == null)
                {
                    Debug.LogWarning($"LocationRoot: у {point.name} не назначен SpawnPointData.", point);
                    continue;
                }

                if (!_spawnPoints.TryAdd(point.Data, point))
                {
                    Debug.LogWarning($"LocationRoot: две точки с одним SpawnPointData ({point.Data.name}).", point);
                }
            }
        }

        public bool TryGetSpawnPoint(SpawnPointData data, out SpawnPoint point)
        {
            point = null;
            return data != null && _spawnPoints.TryGetValue(data, out point);
        }

        /// <summary>Задаёт всем врагам сцены цель и получателя награды. Вызывать до их первого Start.</summary>
        public void InitializeEnemies(PlayerController player)
        {
            RewardReceiver rewardReceiver = player.GetComponent<RewardReceiver>();

            foreach (EnemyController enemy in CollectInScene<EnemyController>())
            {
                enemy.Initialize(player.transform, rewardReceiver);
            }
        }

        private List<T> CollectInScene<T>()
        {
            var result = new List<T>();
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                result.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return result;
        }
        private void OnDestroy()
        {
            GameSession.UnregisterLocation(this);
        }
    }
}