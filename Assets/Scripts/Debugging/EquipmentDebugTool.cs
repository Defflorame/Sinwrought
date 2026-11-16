using System.Collections.Generic;
using UnityEngine;
using Game.Items;
using Game.Player.Equipment;
using Game.Combat;
using Game.Player;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Не часть архитектуры инвентаря/экипировки —
    /// удалите этот файл, когда появится реальный InventoryUI (Phase 14).
    ///
    /// В отличие от предыдущей версии не завязан на фиксированные слоты —
    /// testItems может содержать любое количество предметов любых типов
    /// (несколько одноручных мечей, двуручное оружие, щиты и т.д.), и для
    /// каждого предмета в инвентаре GUI показывает применимые к нему
    /// действия (Equip Main Hand / Equip Off Hand / Equip Ring и т.п.).
    /// Это позволяет проверить любую комбинацию рук, включая то, что
    /// невозможно было проверить с фиксированными F-клавишами.
    /// </summary>
    public class EquipmentDebugTool : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;
        [SerializeField] private PlayerEquipment playerEquipment;
        [SerializeField] private CombatLoadout combatLoadout;
        [SerializeField] private QuickSlotController quickSlots;


        [Header("Тестовые предметы (любое количество, любых типов)")]
        [SerializeField] private ItemData[] testItems;

        private Vector2 _inventoryScroll;
        private Vector2 _equippedScroll;

        private void OnGUI()
        {
            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            label.normal.textColor = Color.white;
            GUIStyle header = new GUIStyle(label) { fontSize = 14, fontStyle = FontStyle.Bold };
            GUIStyle small = new GUIStyle(label) { fontSize = 12 };

            GUI.Box(new Rect(10, 10, 900, 420), "");
            GUILayout.BeginArea(new Rect(20, 20, 880, 400));
            GUILayout.BeginHorizontal();

            // --- Левая колонка: добавление тестовых предметов + инвентарь ---
            GUILayout.BeginVertical(GUILayout.Width(430));
            GUILayout.Label("Тестовые предметы", header);
            if (GUILayout.Button("Добавить все в инвентарь (ещё по одному)", GUILayout.Height(28)))
            {
                AddAllTestItemsToInventory();
            }
            GUILayout.Space(6);
            GUILayout.Label("Быстрые слоты", header);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < QuickSlotController.SlotCount; i++)
            {
                ConsumableData slotData = quickSlots.GetSlot(i);
                string slotText = slotData != null ? $"{i + 1}: {slotData.itemName} x{quickSlots.GetCount(i)}" : $"{i + 1}: -";
                GUILayout.Label(slotText, small, GUILayout.Width(120));
                GUI.enabled = slotData != null;
                if (GUILayout.Button("x", GUILayout.Width(22))) quickSlots.Clear(i);
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Инвентарь", header);
            _inventoryScroll = GUILayout.BeginScrollView(_inventoryScroll, GUILayout.Height(270));

            foreach (ItemStack stack in new List<ItemStack>(inventory.GetAllItems()))
            {
                DrawInventoryStackControls(stack, label, small);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(15);

            // --- Правая колонка: текущая экипировка ---
            GUILayout.BeginVertical(GUILayout.Width(400));
            GUILayout.Label("Экипировано", header);

            HandEquipment hands = playerEquipment.Hands;

            DrawEquippedHandRow("MainHand", hands.MainHandItem, () => playerEquipment.UnequipMainHandWeapon(), label);
            DrawEquippedHandRow("OffHand", hands.OffHandItem, () => playerEquipment.UnequipOffHand(), label);

            GUILayout.Label($"OffHand locked (двуручное в MainHand): {hands.IsOffHandLocked}", small);
            GUILayout.Label($"Active weapon: {combatLoadout.CurrentWeapon?.itemName ?? "-"}", label);
            GUILayout.Label($"Dual-wielding: {combatLoadout.IsDualWielding}", small);

            GUI.enabled = combatLoadout.IsDualWielding;
            if (GUILayout.Button("Switch Active Weapon"))
            {
                combatLoadout.TrySwitchActiveWeapon();
            }
            GUI.enabled = true;

            GUILayout.Space(6);
            if (GUILayout.Button("Разоружить полностью (MainHand + OffHand)"))
            {
                playerEquipment.UnequipMainHandWeapon();
                playerEquipment.UnequipOffHand();
            }

            GUILayout.Space(10);
            GUILayout.Label("Слоты брони / аксессуара / колец", header);
            _equippedScroll = GUILayout.BeginScrollView(_equippedScroll, GUILayout.Height(150));

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                ItemInstance equipped = playerEquipment.GetEquipped(slot);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{slot}: {equipped?.Data.itemName ?? "-"}", label, GUILayout.Width(220));
                GUI.enabled = equipped != null;
                if (GUILayout.Button("Unequip", GUILayout.Width(80)))
                {
                    playerEquipment.Unequip(slot);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            foreach (ItemInstance ring in new List<ItemInstance>(playerEquipment.EquippedRings))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Ring: {ring.Data.itemName}", label, GUILayout.Width(220));
                if (GUILayout.Button("Unequip", GUILayout.Width(80)))
                {
                    playerEquipment.UnequipRing(ring);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.Label($"Total weight: {playerEquipment.GetTotalWeight():F1}", label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void AddAllTestItemsToInventory()
        {
            if (testItems == null) return;

            foreach (ItemData data in testItems)
            {
                if (data == null) continue;
                inventory.AddItem(new ItemInstance(data));
            }

            Debug.Log("EquipmentDebugTool: тестовые предметы добавлены в инвентарь.");
        }

        private void DrawInventoryStackControls(ItemStack stack, GUIStyle label, GUIStyle small)
        {
            ItemInstance item = stack.Item;
            ItemData data = item.Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{data.itemName} x{stack.Quantity}", label, GUILayout.Width(160));

            switch (data)
            {
                case WeaponData weaponData:
                    if (GUILayout.Button("→ Main Hand", GUILayout.Width(100)))
                    {
                        playerEquipment.EquipMainHandWeapon(item);
                    }

                    GUI.enabled = !weaponData.isTwoHanded && !playerEquipment.Hands.IsOffHandLocked;
                    if (GUILayout.Button("→ Off Hand", GUILayout.Width(100)))
                    {
                        playerEquipment.EquipOffHandWeapon(item);
                    }
                    GUI.enabled = true;

                    if (weaponData.isTwoHanded)
                    {
                        GUILayout.Label("(2H)", small, GUILayout.Width(30));
                    }
                    break;

                case ShieldData _:
                    GUI.enabled = !playerEquipment.Hands.IsOffHandLocked;
                    if (GUILayout.Button("→ Off Hand", GUILayout.Width(100)))
                    {
                        playerEquipment.EquipShield(item);
                    }
                    GUI.enabled = true;
                    break;

                case ArmorData armorData:
                    if (GUILayout.Button($"→ {armorData.slot}", GUILayout.Width(120)))
                    {
                        playerEquipment.Equip(item, armorData.slot);
                    }
                    break;

                case AccessoryData _:
                    if (GUILayout.Button("→ Accessory", GUILayout.Width(100)))
                    {
                        playerEquipment.Equip(item, EquipmentSlot.Accessory);
                    }
                    break;

                case RingData _:
                    if (GUILayout.Button("→ Ring", GUILayout.Width(100)))
                    {
                        playerEquipment.EquipRing(item);
                    }
                    break;
                case ConsumableData consumable:
                    for (int i = 0; i < QuickSlotController.SlotCount; i++)
                    {
                        if (GUILayout.Button($"→ {i + 1}", GUILayout.Width(44)))
                        {
                            quickSlots.TryAssign(i, consumable);
                        }
                    }
                    break;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawEquippedHandRow(string label, ItemInstance item, System.Action unequip, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {item?.Data.itemName ?? "-"}", style, GUILayout.Width(260));
            GUI.enabled = item != null;
            if (GUILayout.Button("Unequip", GUILayout.Width(80)))
            {
                unequip();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}