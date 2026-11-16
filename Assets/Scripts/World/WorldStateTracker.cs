using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Save;

namespace Game.World
{
    /// <summary>
    /// Что в мире изменено игроком: множество Id «активных» объектов (открытые ворота, позже подобранные
    /// предметы и сундуки). Что означает «активен», знает сам объект. Живёт на игроке и переживает смену сцен.
    /// Restore заменяет множество целиком и сообщает объектам сцены событием.
    /// </summary>
    public class WorldStateTracker : MonoBehaviour, ISaveable
    {
        [Serializable]
        private class WorldSaveData
        {
            public List<string> activeIds = new List<string>();
        }

        private readonly HashSet<string> _activeIds = new HashSet<string>();

        /// <summary>Трекер текущего игрока. Объекты сцены берут его отсюда, а не ищут по сцене.</summary>
        public static WorldStateTracker Current { get; private set; }

        /// <summary>Появился трекер: объекты, стартовавшие раньше игрока, подключаются по нему.</summary>
        public static event Action<WorldStateTracker> Registered;

        /// <summary>Состояние заменено из сохранения: объекты сцены должны пересинхронизироваться.</summary>
        public event Action OnRestored;

        public string SaveKey => "world";
        public int RestoreOrder => 50;

        private void Awake()
        {
            Current = this;
            Registered?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public bool IsActive(string id) => !string.IsNullOrEmpty(id) && _activeIds.Contains(id);

        public void SetActive(string id, bool active)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("WorldStateTracker: пустой Id, состояние не записано.", this);
                return;
            }

            if (active) _activeIds.Add(id);
            else _activeIds.Remove(id);
        }

        public string Capture()
        {
            return JsonUtility.ToJson(new WorldSaveData { activeIds = new List<string>(_activeIds) });
        }

        public void Restore(string json)
        {
            WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);
            if (data == null) return;

            _activeIds.Clear();
            foreach (string id in data.activeIds)
            {
                if (!string.IsNullOrEmpty(id)) _activeIds.Add(id);
            }

            OnRestored?.Invoke();
        }
    }
}