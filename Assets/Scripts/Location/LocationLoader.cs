using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Player;

namespace Game.Locations
{
    /// <summary>
    /// Загружает сцену локации. Живёт на игроке, поэтому переживает загрузку.
    /// На время загрузки отключает управление игроком. Без корутин: AsyncOperation.completed.
    /// </summary>
    public class LocationLoader : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;

        public bool IsLoading { get; private set; }

        /// <summary>Сцена загружена, игрок включён. Подписчик может принять решение о его состоянии (загрузка сохранения).</summary>
        public event Action OnLoaded;

        /// <summary>false — загрузка не начата (уже идёт другая, пустые данные или сцены нет в Build Settings).</summary>
        public bool TryLoad(LocationData location, SpawnPointData spawnPoint)
        {
            if (IsLoading || location == null || spawnPoint == null) return false;

            if (!Application.CanStreamedLevelBeLoaded(location.SceneName))
            {
                Debug.LogError($"LocationLoader: сцена '{location.SceneName}' не найдена. Добавьте её в Build Settings.", this);
                return false;
            }

            IsLoading = true;
            playerController.DisablePlayer();
            GameSession.SetPendingSpawn(spawnPoint);

            AsyncOperation operation = SceneManager.LoadSceneAsync(location.SceneName);
            operation.completed += HandleLoadCompleted;
            return true;
        }

        private void HandleLoadCompleted(AsyncOperation operation)
        {
            IsLoading = false;
            playerController.EnablePlayer();
            OnLoaded?.Invoke();
        }
    }
}