using System;
using UnityEngine;
using Game.Items;

namespace Game.World
{
    /// <summary>
    /// Предмет в мире: подбирается один раз по кнопке Interact. О сохранении не знает:
    /// запоминает подбор PersistentPickupState, подписанный на OnPickedUp.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData item;
        [Min(1)]
        [SerializeField] private int quantity = 1;
        [SerializeField] private string prompt = "Подобрать";

        [Tooltip("Дочерний объект с моделью и коллайдером. Скрывается после подбора. Не корень: на корне живёт PersistentPickupState, он должен оставаться активным.")]
        [SerializeField] private GameObject visualRoot;

        public bool Taken { get; private set; }

        /// <summary>Игрок подобрал предмет. При восстановлении из сохранения не вызывается.</summary>
        public event Action OnPickedUp;

        private void Awake()
        {
            if (visualRoot == null)
            {
                Debug.LogWarning($"ItemPickup: у {name} не назначен visualRoot — после подбора объект останется видимым.", this);
            }
        }

        public bool CanInteract(GameObject interactor) => !Taken && item != null;

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;

            Inventory inventory = interactor.GetComponentInChildren<Inventory>();
            if (inventory == null)
            {
                Debug.LogWarning("ItemPickup: у игрока нет Inventory.", this);
                return;
            }

            inventory.AddItem(new ItemInstance(item), quantity);
            Debug.Log($"ItemPickup: подобрано {item.name} x{quantity}");

            SetTaken(true);
            OnPickedUp?.Invoke();
        }

        /// <summary>Ставит состояние из сохранения: без выдачи предмета и без OnPickedUp.</summary>
        public void RestoreTaken(bool taken) => SetTaken(taken);

        /// <summary>Задаёт предмет и количество для подбора, созданного из кода (выпавший лут).</summary>
        public void Configure(ItemData newItem, int newQuantity)
        {
            item = newItem;
            quantity = Mathf.Max(1, newQuantity);
        }
        public string GetInteractionPrompt()
        {
            return item != null ? $"{prompt}: {item.itemName}" : prompt;
        }

        private void SetTaken(bool taken)
        {
            Taken = taken;
            if (visualRoot != null) visualRoot.SetActive(!taken);
        }

    }
}