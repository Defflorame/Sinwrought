using UnityEngine;
using Game.Player;

namespace Game.Locations
{
    /// <summary>
    /// Единственный статический держатель сессии: текущий игрок, переживающий загрузку сцен.
    /// Сбрасывается при старте игры независимо от настройки Domain Reload.
    /// </summary>
    public static class GameSession
    {
        public static PlayerController Player { get; private set; }
        public static LocationRoot CurrentLocationRoot { get; private set; }

        private static SpawnPointData _pendingSpawn;

        public static void RegisterPlayer(PlayerController player)
        {
            Player = player;
        }

        public static void SetPendingSpawn(SpawnPointData spawnPoint)
        {
            _pendingSpawn = spawnPoint;
        }

        /// <summary>Возвращает запрошенную точку входа и сразу забывает её (одноразово).</summary>
        public static SpawnPointData TakePendingSpawn()
        {
            SpawnPointData spawnPoint = _pendingSpawn;
            _pendingSpawn = null;
            return spawnPoint;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Player = null;
            _pendingSpawn = null;
            CurrentLocationRoot = null;
        }
        public static void RegisterLocation(LocationRoot root)
        {
            CurrentLocationRoot = root;
        }

        public static void UnregisterLocation(LocationRoot root)
        {
            if (CurrentLocationRoot == root) CurrentLocationRoot = null;
        }
    }
}