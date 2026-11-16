using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Items;
using Game.Save;

namespace Game.Player
{
    /// <summary>
    /// Три быстрых слота расходников. Слот хранит ссылку на ConsumableData (не на экземпляр): количество
    /// берётся из Inventory. Опустевший стак слот не очищает. Назначает слоты вызывающий код
    /// (отладочная панель, позже UI инвентаря). Живёт на корне игрока: он же владелец для эффектов.
    /// </summary>
    public class QuickSlotController : MonoBehaviour, ISaveable
    {
        public const int SlotCount = 3;

        [SerializeField] private Inventory inventory;
        [Tooltip("Нужен для восстановления слотов по Id из файла сохранения.")]
        [SerializeField] private ItemCatalogData itemCatalog;

        private readonly ConsumableData[] _slots = new ConsumableData[SlotCount];

        public event Action OnSlotsChanged;

        public string SaveKey => "quickSlots";
        public int RestoreOrder => 15;

        public ConsumableData GetSlot(int index) => IsValidIndex(index) ? _slots[index] : null;

        /// <summary>Сколько предметов слота лежит в инвентаре (0 для пустого слота).</summary>
        public int GetCount(int index)
        {
            ConsumableData data = GetSlot(index);
            return data != null ? inventory.GetItemCount(data) : 0;
        }

        /// <summary>Кладёт расходник в слот. Если он уже лежал в другом слоте, оттуда убирается.</summary>
        public bool TryAssign(int index, ConsumableData data)
        {
            if (!IsValidIndex(index) || data == null) return false;

            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] == data) _slots[i] = null;
            }

            _slots[index] = data;
            OnSlotsChanged?.Invoke();
            return true;
        }

        public void Clear(int index)
        {
            if (!IsValidIndex(index) || _slots[index] == null) return;

            _slots[index] = null;
            OnSlotsChanged?.Invoke();
        }

        public bool CanUse(int index)
        {
            return ConsumableUseService.CanUse(GetSlot(index), gameObject, inventory);
        }

        /// <summary>Использует расходник из слота. false — слот пуст, предмета нет или он сейчас бесполезен.</summary>
        public bool TryUse(int index)
        {
            bool used = ConsumableUseService.TryUse(GetSlot(index), gameObject, inventory);
            if (used) OnSlotsChanged?.Invoke();
            return used;
        }

        private static bool IsValidIndex(int index) => index >= 0 && index < SlotCount;

        [Serializable]
        private class SaveData
        {
            // Id предмета в каждом слоте, 0 — слот пуст.
            public List<int> itemIds = new List<int>();
        }

        public string Capture()
        {
            var data = new SaveData();

            foreach (ConsumableData slot in _slots)
            {
                if (slot != null && slot.Id == 0)
                {
                    Debug.LogWarning($"QuickSlotController: у '{slot.name}' нет Id, слот не сохранён.", this);
                }

                data.itemIds.Add(slot != null ? slot.Id : 0);
            }

            return JsonUtility.ToJson(data);
        }

        /// <summary>Заменяет слоты сохранёнными. Неизвестные предметы пропускаются.</summary>
        public void Restore(string json)
        {
            if (itemCatalog == null)
            {
                Debug.LogWarning("QuickSlotController: не назначен каталог предметов, восстановление невозможно.", this);
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            for (int i = 0; i < SlotCount; i++)
            {
                _slots[i] = null;
                if (i >= data.itemIds.Count || data.itemIds[i] == 0) continue;

                if (itemCatalog.TryGet(data.itemIds[i], out ItemData item) && item is ConsumableData consumable)
                {
                    _slots[i] = consumable;
                }
                else
                {
                    Debug.LogWarning($"QuickSlotController: расходник с Id {data.itemIds[i]} не найден в каталоге.", this);
                }
            }

            OnSlotsChanged?.Invoke();
        }
    }
}