using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Делает состояние ворот постоянным: записывает открытие/закрытие в WorldStateTracker и применяет
    /// записанное, когда сцена появляется или игра загружается. Ворота с автозакрытием не сохраняются
    /// (временный барьер). Чтобы сохранять новые ворота, достаточно добавить этот компонент.
    /// </summary>
    [RequireComponent(typeof(WorldGate), typeof(PersistentId))]
    public class PersistentGateState : MonoBehaviour
    {
        private WorldGate _gate;
        private PersistentId _id;
        private WorldStateTracker _tracker;

        private void Awake()
        {
            _gate = GetComponent<WorldGate>();
            _id = GetComponent<PersistentId>();
        }

        private void OnEnable()
        {
            _gate.OnOpened += HandleOpened;
            _gate.OnClosed += HandleClosed;
            WorldStateTracker.Registered += Bind;

            if (WorldStateTracker.Current != null) Bind(WorldStateTracker.Current);
        }

        private void OnDisable()
        {
            _gate.OnOpened -= HandleOpened;
            _gate.OnClosed -= HandleClosed;
            WorldStateTracker.Registered -= Bind;

            if (_tracker != null)
            {
                _tracker.OnRestored -= Sync;
                _tracker = null;
            }
        }

        private void Bind(WorldStateTracker tracker)
        {
            if (_tracker == tracker) return;

            if (_tracker != null) _tracker.OnRestored -= Sync;

            _tracker = tracker;
            _tracker.OnRestored += Sync;
            Sync();
        }

        /// <summary>Приводит ворота к записанному состоянию, не трогая требования и расход ключа.</summary>
        private void Sync()
        {
            if (_gate.AutoCloses || _tracker == null) return;

            bool shouldBeOpen = _tracker.IsActive(_id.Id);
            if (_gate.IsOpen != shouldBeOpen) _gate.RestoreState(shouldBeOpen);
        }

        private void HandleOpened() => Record(true);
        private void HandleClosed() => Record(false);

        private void Record(bool open)
        {
            if (_gate.AutoCloses || _tracker == null) return;

            _tracker.SetActive(_id.Id, open);
        }
    }
}