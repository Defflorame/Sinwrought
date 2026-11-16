using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Items;
using Game.Player.Stats;
using Game.Stats;
using Game.Combat;
using Game.Save;

namespace Game.Player.Equipment
{
    /// <summary>
    /// Координатор экипировки игрока: слоты брони/аксессуара, кольца
    /// (список без жёсткого enum-слота на каждое — раздел 37: "количество
    /// колец не должно быть захардкожено в Combat System") и делегирование
    /// рук в HandEquipment. Обмен предметами с Inventory и применение
    /// StatModifier через EquipmentModifierService происходит здесь —
    /// HandEquipment и Inventory сами об этом не знают.
    /// </summary>
    public class PlayerEquipment : MonoBehaviour, ISaveable
    {
        [SerializeField] private HandEquipment handEquipment;
        [SerializeField] private Inventory inventory;
        [SerializeField] private CharacterStats characterStats;

        [Tooltip("Максимальное количество одновременно надетых колец.")]
        [SerializeField] private int maxRingSlots = 2;

        private readonly Dictionary<EquipmentSlot, ItemInstance> _slots = new Dictionary<EquipmentSlot, ItemInstance>();
        private readonly List<ItemInstance> _equippedRings = new List<ItemInstance>();

        public event Action OnEquipmentChanged;

        public HandEquipment Hands => handEquipment;
        public IReadOnlyList<ItemInstance> EquippedRings => _equippedRings;

        [SerializeField] private ElementResistanceModifiers resistanceModifiers;

        [Tooltip("Нужен для восстановления предметов по Id из файла сохранения.")]
        [SerializeField] private ItemCatalogData itemCatalog;

        public string SaveKey => "equipment";
        public int RestoreOrder => 20;

        public ItemInstance GetEquipped(EquipmentSlot slot)
        {
            return _slots.TryGetValue(slot, out ItemInstance item) ? item : null;
        }

        public bool Equip(ItemInstance item, EquipmentSlot slot)
        {
            if (!CanEquipToSlot(item, slot))
            {
                Debug.LogWarning($"PlayerEquipment: {item?.Data?.itemName} нельзя экипировать в слот {slot}.", this);
                return false;
            }

            if (!TryTakeFromInventory(item)) return false;
            UnequipInternal(slot);

            _slots[slot] = item;
            EquipmentModifierService.ApplyItemModifiers(characterStats, item);
            EquipmentModifierService.ApplyItemResistances(resistanceModifiers, item);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public ItemInstance Unequip(EquipmentSlot slot)
        {
            ItemInstance item = UnequipInternal(slot);
            if (item != null)
            {
                OnEquipmentChanged?.Invoke();
            }

            return item;
        }

        public bool EquipRing(ItemInstance item)
        {
            if (item == null || !(item.Data is RingData))
            {
                Debug.LogWarning($"PlayerEquipment: {item?.Data?.itemName} не является кольцом.", this);
                return false;
            }

            if (_equippedRings.Count >= maxRingSlots)
            {
                Debug.Log("PlayerEquipment: нет свободных слотов для колец.");
                return false;
            }

            if (!TryTakeFromInventory(item)) return false;
            _equippedRings.Add(item);
            EquipmentModifierService.ApplyItemModifiers(characterStats, item);
            EquipmentModifierService.ApplyItemResistances(resistanceModifiers, item);   // новая
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public bool UnequipRing(ItemInstance item)
        {
            if (item == null || !_equippedRings.Remove(item)) return false;

            EquipmentModifierService.RemoveItemModifiers(characterStats, item);
            EquipmentModifierService.RemoveItemResistances(resistanceModifiers, item);  // новая
            inventory.AddItem(item);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        /// <summary>Экипирует оружие в MainHand. Если оружие двуручное — освобождает OffHand.</summary>
        public bool EquipMainHandWeapon(ItemInstance item)
        {
            if (!IsItemOfType<WeaponData>(item, "оружием")) return false;

            if (!TryTakeFromInventory(item)) return false;
            bool success = handEquipment.EquipMainHandWeapon(item, out ItemInstance previousMainHand, out ItemInstance displacedOffHand);
            return FinishHandEquip(success, item, previousMainHand, displacedOffHand);
        }

        public void UnequipMainHandWeapon()
        {
            ItemInstance previous = handEquipment.UnequipMainHand();
            if (previous != null)
            {
                inventory.AddItem(previous);
                OnEquipmentChanged?.Invoke();
            }
        }

        /// <summary>Второе одноручное оружие в OffHand (dual-wield).</summary>
        public bool EquipOffHandWeapon(ItemInstance item)
        {
            if (!IsItemOfType<WeaponData>(item, "оружием")) return false;

            if (!TryTakeFromInventory(item)) return false;
            bool success = handEquipment.EquipOffHandWeapon(item, out ItemInstance displaced);
            return FinishHandEquip(success, item, displaced);
        }

        public bool EquipShield(ItemInstance item)
        {
            if (!IsItemOfType<ShieldData>(item, "щитом")) return false;

            if (!TryTakeFromInventory(item)) return false;
            bool success = handEquipment.EquipShield(item, out ItemInstance displaced);
            return FinishHandEquip(success, item, displaced);
        }

        /// <summary>Снимает то, что сейчас в OffHand (щит или второе оружие).</summary>
        public void UnequipOffHand()
        {
            ItemInstance previous = handEquipment.UnequipOffHand();
            if (previous != null)
            {
                inventory.AddItem(previous);
                OnEquipmentChanged?.Invoke();
            }
        }

        public float GetTotalWeight()
        {
            return EquipmentModifierService.GetTotalWeight(_slots.Values, _equippedRings, handEquipment.MainHandItem, handEquipment.OffHandItem);
        }

        public float GetLoadRatio()
        {
            return EncumbranceService.GetLoadRatio(GetTotalWeight(), characterStats.Stats.CarryWeightLimit);
        }

        /// <summary>
        /// Забирает из инвентаря именно этот экземпляр. false — его там нет: экипировать
        /// нельзя, иначе предмет появится «из воздуха» (дубликат).
        /// </summary>
        private bool TryTakeFromInventory(ItemInstance item)
        {
            if (item != null && inventory.RemoveItem(item)) return true;

            Debug.LogWarning($"PlayerEquipment: {item?.Data?.itemName} нет в инвентаре — экипировать нельзя.", this);
            return false;
        }

        private bool CanEquipToSlot(ItemInstance item, EquipmentSlot slot)
        {
            if (item == null) return false;

            if (item.Data is ArmorData armorData)
            {
                return armorData.slot == slot;
            }

            if (item.Data is AccessoryData)
            {
                return slot == EquipmentSlot.Accessory;
            }

            return false;
        }
        private bool IsItemOfType<T>(ItemInstance item, string typeName) where T : ItemData
        {
            if (item != null && item.Data is T) return true;

            Debug.LogWarning($"PlayerEquipment: {item?.Data?.itemName} не является {typeName}.", this);
            return false;
        }

        /// <summary>Общий хвост экипировки в руки: при неудаче вернуть предмет в инвентарь, иначе вернуть вытесненные.</summary>
        private bool FinishHandEquip(bool success, ItemInstance item, ItemInstance displacedA, ItemInstance displacedB = null)
        {
            if (!success)
            {
                inventory.AddItem(item);
                return false;
            }

            if (displacedA != null) inventory.AddItem(displacedA);
            if (displacedB != null) inventory.AddItem(displacedB);

            OnEquipmentChanged?.Invoke();
            return true;
        }

        private ItemInstance UnequipInternal(EquipmentSlot slot)
        {
            if (!_slots.TryGetValue(slot, out ItemInstance item) || item == null)
            {
                return null;
            }

            EquipmentModifierService.RemoveItemModifiers(characterStats, item);
            EquipmentModifierService.RemoveItemResistances(resistanceModifiers, item);
            _slots.Remove(slot);
            inventory.AddItem(item);
            return item;
        }

        [Serializable]
        private class SlotSave
        {
            public string slot;
            public int itemId;
        }

        [Serializable]
        private class SaveData
        {
            public List<SlotSave> slots = new List<SlotSave>();
            public List<int> rings = new List<int>();
            public int mainHandId;
            public int offHandId;
            public string activeHand;
        }

        public string Capture()
        {
            var data = new SaveData
            {
                mainHandId = IdOf(handEquipment.MainHandItem),
                offHandId = IdOf(handEquipment.OffHandItem),
                activeHand = handEquipment.ActiveWeaponHand.ToString()
            };

            foreach (KeyValuePair<EquipmentSlot, ItemInstance> pair in _slots)
            {
                int id = IdOf(pair.Value);
                if (id != 0) data.slots.Add(new SlotSave { slot = pair.Key.ToString(), itemId = id });
            }

            foreach (ItemInstance ring in _equippedRings)
            {
                int id = IdOf(ring);
                if (id != 0) data.rings.Add(id);
            }

            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Заменяет экипировку сохранённой. Инвентарь не трогает (его секция восстанавливается отдельно),
        /// поэтому не использует Equip*: те забирают предмет из рюкзака.
        /// </summary>
        public void Restore(string json)
        {
            if (itemCatalog == null)
            {
                Debug.LogWarning("PlayerEquipment: не назначен каталог предметов, восстановление невозможно.", this);
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            ClearEquipped();

            foreach (SlotSave saved in data.slots)
            {
                if (!Enum.TryParse(saved.slot, out EquipmentSlot slot)) continue;

                ItemInstance item = CreateInstance(saved.itemId);
                if (item == null) continue;

                if (!CanEquipToSlot(item, slot))
                {
                    Debug.LogWarning($"PlayerEquipment: {item.Data.itemName} не подходит в слот {slot}, пропущен.", this);
                    continue;
                }

                _slots[slot] = item;
                ApplyBonuses(item);
            }

            foreach (int id in data.rings)
            {
                if (_equippedRings.Count >= maxRingSlots) break;

                ItemInstance ring = CreateInstance(id);
                if (ring == null || !(ring.Data is RingData)) continue;

                _equippedRings.Add(ring);
                ApplyBonuses(ring);
            }

            Enum.TryParse(data.activeHand, out WeaponHand activeHand);
            handEquipment.RestoreHands(CreateInstance(data.mainHandId), CreateInstance(data.offHandId), activeHand);

            OnEquipmentChanged?.Invoke();
        }

        private void ClearEquipped()
        {
            foreach (ItemInstance item in _slots.Values) RemoveBonuses(item);
            _slots.Clear();

            foreach (ItemInstance ring in _equippedRings) RemoveBonuses(ring);
            _equippedRings.Clear();
        }

        private void ApplyBonuses(ItemInstance item)
        {
            EquipmentModifierService.ApplyItemModifiers(characterStats, item);
            EquipmentModifierService.ApplyItemResistances(resistanceModifiers, item);
        }

        private void RemoveBonuses(ItemInstance item)
        {
            EquipmentModifierService.RemoveItemModifiers(characterStats, item);
            EquipmentModifierService.RemoveItemResistances(resistanceModifiers, item);
        }

        private ItemInstance CreateInstance(int itemId)
        {
            if (itemId == 0) return null;

            if (itemCatalog.TryGet(itemId, out ItemData itemData)) return new ItemInstance(itemData);

            Debug.LogWarning($"PlayerEquipment: предмет с Id {itemId} не найден в каталоге.", this);
            return null;
        }

        private static int IdOf(ItemInstance item) => item?.Data != null ? item.Data.Id : 0;
    }
}