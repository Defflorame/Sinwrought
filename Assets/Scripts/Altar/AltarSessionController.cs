using System;
using UnityEngine;
using Game.Combat;
using Game.Locations;
using Game.Player;
using Game.Progression;
using Game.Save;

namespace Game.Altars
{
    /// <summary>
    /// Сессия игрока у алтаря. Begin: отдых (полное восстановление), запись последнего алтаря,
    /// разблокировка fast travel (если условия выполнены), остановка игрока, карта ввода UI с курсором,
    /// открытие траты очков. End: всё обратно. TryFastTravel: закрывает сессию и переносит игрока.
    /// TrySave: ручное сохранение (кнопка в меню алтаря).
    /// Сам ничего не рисует: меню и кнопки — забота UI, который вызывает команды.
    /// </summary>
    public class AltarSessionController : MonoBehaviour
    {
        [SerializeField] private AltarTracker tracker;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerCombatController combatController;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UIInputReader uiInput;
        [SerializeField] private PointSpendingGate gate;
        [SerializeField] private LocationTravelController travel;
        [SerializeField] private SaveController saveController;

        public bool IsActive { get; private set; }
        public AltarData CurrentAltar { get; private set; }

        /// <summary>Игрок отдохнул у алтаря. На неё подпишется карта (10.M2).</summary>
        public event Action<AltarData> OnAltarRested;

        private void OnEnable()
        {
            uiInput.OnCancelPressed += End;
        }

        private void OnDisable()
        {
            uiInput.OnCancelPressed -= End;
        }

        public void Begin(AltarData altar)
        {
            if (altar == null || IsActive) return;

            // Оборвать блок/заряд и т.п.: иначе состояние зависнет, пока игрок отключён.
            combatController.CancelAllActions();

            VitalsRestoreService.RestoreAll(gameObject);
            tracker.RecordRest(altar);

            // Условие проверяется без Consume; после выполнения флаг остаётся навсегда.
            if (FastTravelService.IsUnlockConditionMet(altar, gameObject))
            {
                tracker.UnlockFastTravel(altar);
            }

            CurrentAltar = altar;
            IsActive = true;

            playerController.DisablePlayer();
            inputManager.SwitchToUI();
            gate.Open();

            Debug.Log($"AltarSessionController: отдых у алтаря '{altar.AltarName}'");
            OnAltarRested?.Invoke(altar);
        }

        public void End()
        {
            if (!IsActive) return;

            gate.Close();
            inputManager.SwitchToPlayer();
            playerController.EnablePlayer();

            IsActive = false;
            CurrentAltar = null;
        }

        public bool CanFastTravelTo(AltarData target)
        {
            return IsActive && FastTravelService.CanTravel(CurrentAltar, target, tracker);
        }

        /// <summary>Сначала закрывает сессию, затем переносит игрока: загрузка сцены не должна застать его с включённым управлением.</summary>
        public bool TryFastTravel(AltarData target)
        {
            if (!CanFastTravelTo(target)) return false;

            End();
            return travel.TryTravel(target.Location, target.SpawnPoint);
        }

        /// <summary>Ручное сохранение. Только при активной сессии. false — не сохранено (нет сессии или ошибка записи).</summary>
        public bool TrySave()
        {
            if (!IsActive || CurrentAltar == null) return false;

            return saveController.SaveNow(CurrentAltar.Id);
        }
    }
}