using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Save;

namespace Game.Items
{
    public class Inventory : MonoBehaviour, ISaveable
    {
        [Tooltip("Нужен для восстановления предметов по Id из файла сохранения.")]
        [SerializeField] private ItemCatalogData itemCatalog;

        private readonly List<ItemStack> _stacks = new List<ItemStack>();

        public event Action OnInventoryChanged;

        public string SaveKey => "inventory";
        public int RestoreOrder => 10;

        public void AddItem(ItemInstance item, int quantity = 1)
        {
            if (AddToStacks(item, quantity)) OnInventoryChanged?.Invoke();
        }

        /// <summary>Раскладывает предмет по стакам без события. false — ничего не добавлено.</summary>
        private bool AddToStacks(ItemInstance item, int quantity)
        {
            if (item == null || item.Data == null || quantity <= 0) return false;

            int maxStack = Mathf.Max(1, item.Data.maxStackSize);

            // 1. Сначала дозаполняем уже существующие неполные стаки.
            if (maxStack > 1)
            {
                foreach (ItemStack stack in _stacks)
                {
                    if (stack.Item.Data != item.Data || stack.Quantity >= maxStack) continue;

                    int toAdd = Mathf.Min(maxStack - stack.Quantity, quantity);
                    stack.Quantity += toAdd;
                    quantity -= toAdd;

                    if (quantity <= 0) break;
                }
            }

            // 2. Остаток — новые стаки не больше maxStack. Для нестакающихся предметов
            //    (maxStack == 1) каждая единица получает собственный ItemInstance.
            bool isFirstNewStack = true;
            while (quantity > 0)
            {
                int amount = Mathf.Min(maxStack, quantity);
                ItemInstance instance = isFirstNewStack ? item : new ItemInstance(item.Data);

                _stacks.Add(new ItemStack(instance, amount));
                quantity -= amount;
                isFirstNewStack = false;
            }

            return true;
        }

        /// <summary>
        /// Нестакающийся предмет: убирает именно этот экземпляр (quantity должно быть 1).
        /// Стакающийся: убирает quantity штук из любых стаков этого типа.
        /// false — ничего не изменено (предмета нет или его недостаточно).
        /// </summary>
        public bool RemoveItem(ItemInstance item, int quantity = 1)
        {
            if (item == null || item.Data == null || quantity <= 0) return false;

            if (item.Data.maxStackSize <= 1)
            {
                if (quantity != 1) return false;

                int index = _stacks.FindIndex(s => s.Item == item);
                if (index < 0) return false;

                _stacks.RemoveAt(index);
                OnInventoryChanged?.Invoke();
                return true;
            }

            if (GetItemCount(item.Data) < quantity) return false;

            for (int i = _stacks.Count - 1; i >= 0 && quantity > 0; i--)
            {
                ItemStack stack = _stacks[i];
                if (stack.Item.Data != item.Data) continue;

                int taken = Mathf.Min(stack.Quantity, quantity);
                stack.Quantity -= taken;
                quantity -= taken;

                if (stack.Quantity <= 0)
                {
                    _stacks.RemoveAt(i);
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(ItemData data) => GetItemCount(data) > 0;

        /// <summary>Суммарное количество по ВСЕМ стакам этого типа предмета.</summary>
        public int GetItemCount(ItemData data)
        {
            int total = 0;
            foreach (ItemStack stack in _stacks)
            {
                if (stack.Item.Data == data) total += stack.Quantity;
            }

            return total;
        }

        public ItemStack FindItem(ItemData data) => _stacks.Find(s => s.Item.Data == data);

        public IReadOnlyList<ItemStack> GetAllItems() => _stacks;
        
        [Serializable]
        private class StackSave
        {
            public int itemId;
            public int quantity;
        }

        [Serializable]
        private class SaveData
        {
            public List<StackSave> stacks = new List<StackSave>();
        }

        public string Capture()
        {
            var data = new SaveData();

            foreach (ItemStack stack in _stacks)
            {
                if (stack.Item.Data.Id == 0)
                {
                    Debug.LogWarning($"Inventory: у предмета '{stack.Item.Data.name}' нет Id, он не сохранён.", this);
                    continue;
                }

                data.stacks.Add(new StackSave { itemId = stack.Item.Data.Id, quantity = stack.Quantity });
            }

            return JsonUtility.ToJson(data);
        }

        /// <summary>Заменяет содержимое рюкзака сохранённым. Предметы, которых нет в каталоге, пропускаются.</summary>
        public void Restore(string json)
        {
            if (itemCatalog == null)
            {
                Debug.LogWarning("Inventory: не назначен каталог предметов, восстановление невозможно.", this);
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            _stacks.Clear();

            foreach (StackSave saved in data.stacks)
            {
                if (!itemCatalog.TryGet(saved.itemId, out ItemData itemData))
                {
                    Debug.LogWarning($"Inventory: предмет с Id {saved.itemId} не найден в каталоге.", this);
                    continue;
                }

                AddToStacks(new ItemInstance(itemData), saved.quantity);
            }

            OnInventoryChanged?.Invoke();
        }
    }

}
