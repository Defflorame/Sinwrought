using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Делает подбор предмета постоянным: записывает его в WorldStateTracker и скрывает/показывает предмет
    /// при появлении сцены и при загрузке. Чтобы предмет помнился, достаточно добавить этот компонент.
    /// </summary>
    [RequireComponent(typeof(ItemPickup), typeof(PersistentId))]
    public class PersistentPickupState : MonoBehaviour
    {
        private ItemPickup _pickup;
        private PersistentId _id;
        private WorldStateTracker _tracker;

        private void Awake()
        {
            _pickup = GetComponent<ItemPickup>();
            _id = GetComponent<PersistentId>();
        }

        private void OnEnable()
        {
            _pickup.OnPickedUp += HandlePickedUp;
            WorldStateTracker.Registered += Bind;

            if (WorldStateTracker.Current != null) Bind(WorldStateTracker.Current);
        }

        private void OnDisable()
        {
            _pickup.OnPickedUp -= HandlePickedUp;
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

        /// <summary>Приводит предмет к записанному состоянию: без выдачи в инвентарь.</summary>
        private void Sync()
        {
            if (_tracker == null) return;

            bool shouldBeTaken = _tracker.IsActive(_id.Id);
            if (_pickup.Taken != shouldBeTaken) _pickup.RestoreTaken(shouldBeTaken);
        }

        private void HandlePickedUp()
        {
            if (_tracker != null) _tracker.SetActive(_id.Id, true);
        }
    }
}