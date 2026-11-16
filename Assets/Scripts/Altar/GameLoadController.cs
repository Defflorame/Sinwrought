// Game.Altars/GameLoadController.cs
using UnityEngine;
using Game.Combat;
using Game.Locations;
using Game.Player;
using Game.Save;

namespace Game.Altars
{
    /// <summary>
    /// Загрузка последнего сохранения. LoadLast: читает файл, находит алтарь, закрывает сессию алтаря,
    /// запускает загрузку сцены алтаря. Когда сцена загружена (LocationLoader.OnLoaded), применяет секции
    /// к игроку и воскрешает его (полное восстановление ресурсов, чистка статус-эффектов).
    /// Позицию не хранит: игрока ставит на точку алтаря существующий PlayerSceneEntry.
    /// </summary>
    public class GameLoadController : MonoBehaviour
    {
        [SerializeField] private SaveController saveController;
        [SerializeField] private AltarCatalogData altarCatalog;
        [SerializeField] private LocationLoader loader;
        [SerializeField] private AltarSessionController session;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private StatusEffectController statusEffects;

        private SaveGameData _pending;

        /// <summary>Есть действительное сохранение, и алтарь из него найден в каталоге.</summary>
        public bool HasSave => TryGetSave(out _, out _);

        private void OnEnable()
        {
            loader.OnLoaded += HandleLoaded;
        }

        private void OnDisable()
        {
            loader.OnLoaded -= HandleLoaded;
        }

        /// <summary>false — загрузка не начата (нет сохранения, неверный алтарь, загрузка уже идёт).</summary>
        public bool LoadLast()
        {
            if (loader.IsLoading) return false;
            if (!TryGetSave(out SaveGameData data, out AltarData altar)) return false;

            // Сессия алтаря не должна пережить загрузку (карта ввода UI, открытый шлюз очков).
            session.End();

            _pending = data;
            if (!loader.TryLoad(altar.Location, altar.SpawnPoint))
            {
                _pending = null;
                return false;
            }

            return true;
        }

        private void HandleLoaded()
        {
            if (_pending == null) return;

            SaveGameData data = _pending;
            _pending = null;

            saveController.Apply(data);
            statusEffects.ClearAll();
            playerController.HandleRespawn(); // RestoreAll + CancelAllActions + EnablePlayer
        }

        private bool TryGetSave(out SaveGameData data, out AltarData altar)
        {
            altar = null;
            if (!SaveFileService.TryRead(out data)) return false;

            if (!altarCatalog.TryGet(data.altarId, out altar))
            {
                Debug.LogWarning($"GameLoadController: алтарь '{data.altarId}' не найден в каталоге.", this);
                return false;
            }

            return altar.Location != null && altar.SpawnPoint != null;
        }
    }
}