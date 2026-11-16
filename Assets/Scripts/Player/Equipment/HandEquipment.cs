using System;
using UnityEngine;
using Game.Items;

namespace Game.Player.Equipment
{
    public enum WeaponHand
    {
        MainHand,
        OffHand
    }

    /// <summary>
    /// Слоты рук: MainHand и OffHand. Раньше были жёстко "Weapon" и "Shield" —
    /// теперь любая рука может держать оружие или щит, что позволяет
    /// реализовать меч+щит, два меча (с переключением активной руки) и
    /// двуручное оружие (занимает обе руки, см. IsOffHandLocked).
    /// </summary>
    public class HandEquipment : MonoBehaviour
    {
        [Header("Стартовая экипировка (для теста)")]
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField] private ShieldData startingShield;

        public ItemInstance MainHandItem { get; private set; }
        public ItemInstance OffHandItem { get; private set; }
        public WeaponHand ActiveWeaponHand { get; private set; } = WeaponHand.MainHand;

        public WeaponData MainHandWeapon => MainHandItem?.Data as WeaponData;
        public WeaponData OffHandWeapon => OffHandItem?.Data as WeaponData;

        /// <summary>Щит валиден только если в OffHand не оружие (иначе это dual-wield).</summary>
        public ShieldData CurrentShield => OffHandWeapon == null ? OffHandItem?.Data as ShieldData : null;

        /// <summary>Оружие, которым персонаж атакует прямо сейчас.</summary>
        public WeaponData ActiveWeapon => IsDualWielding
            ? (ActiveWeaponHand == WeaponHand.MainHand ? MainHandWeapon : OffHandWeapon)
            : MainHandWeapon;

        public bool IsDualWielding => MainHandWeapon != null && OffHandWeapon != null;

        /// <summary>OffHand недоступен для щита/второго оружия, пока в MainHand двуручное оружие.</summary>
        public bool IsOffHandLocked => MainHandWeapon != null && MainHandWeapon.isTwoHanded;

        public event Action OnHandEquipmentChanged;

        private void Awake()
        {
            if (startingWeapon != null)
            {
                EquipMainHandWeapon(new ItemInstance(startingWeapon), out _, out _);
            }

            if (startingShield != null && !IsOffHandLocked)
            {
                EquipShield(new ItemInstance(startingShield), out _);
            }
        }

        /// <summary>
        /// Если экипируется двуручное оружие — освобождает OffHand.
        /// previousMainHand — что было в MainHand раньше (если было).
        /// displacedOffHand — что было вытеснено из OffHand (если экипируется двуручное).
        /// Оба нужно вернуть в инвентарь — этим занимается PlayerEquipment.
        /// </summary>
        public bool EquipMainHandWeapon(ItemInstance weapon, out ItemInstance previousMainHand, out ItemInstance displacedOffHand)
        {
            previousMainHand = null;
            displacedOffHand = null;

            if (weapon == null || !(weapon.Data is WeaponData weaponData))
            {
                Debug.LogWarning($"HandEquipment: {weapon?.Data?.itemName} не является оружием.", this);
                return false;
            }

            previousMainHand = MainHandItem;

            if (weaponData.isTwoHanded && OffHandItem != null)
            {
                displacedOffHand = OffHandItem;
                OffHandItem = null;
            }

            MainHandItem = weapon;
            ActiveWeaponHand = WeaponHand.MainHand;
            OnHandEquipmentChanged?.Invoke();
            return true;
        }

        public ItemInstance UnequipMainHand()
        {
            ItemInstance previous = MainHandItem;
            MainHandItem = null;
            ActiveWeaponHand = WeaponHand.MainHand;
            OnHandEquipmentChanged?.Invoke();
            return previous;
        }

        /// <summary>Второе одноручное оружие в OffHand (dual-wield). displaced — что было в OffHand раньше.</summary>
        public bool EquipOffHandWeapon(ItemInstance weapon, out ItemInstance displaced)
        {
            displaced = null;

            if (weapon == null || !(weapon.Data is WeaponData weaponData) || weaponData.isTwoHanded)
            {
                Debug.LogWarning($"HandEquipment: {weapon?.Data?.itemName} нельзя надеть во вторую руку.", this);
                return false;
            }

            if (IsOffHandLocked)
            {
                Debug.LogWarning("HandEquipment: OffHand занят двуручным оружием.", this);
                return false;
            }

            displaced = OffHandItem;
            OffHandItem = weapon;
            ActiveWeaponHand = WeaponHand.OffHand;
            OnHandEquipmentChanged?.Invoke();
            return true;
        }

        public bool EquipShield(ItemInstance shield, out ItemInstance displaced)
        {
            displaced = null;

            if (shield == null || !(shield.Data is ShieldData))
            {
                Debug.LogWarning($"HandEquipment: {shield?.Data?.itemName} не является щитом.", this);
                return false;
            }

            if (IsOffHandLocked)
            {
                Debug.LogWarning("HandEquipment: OffHand занят двуручным оружием — сначала снимите его.", this);
                return false;
            }

            displaced = OffHandItem;
            OffHandItem = shield;
            OnHandEquipmentChanged?.Invoke();
            return true;
        }

        public ItemInstance UnequipOffHand()
        {
            ItemInstance previous = OffHandItem;
            OffHandItem = null;
            ActiveWeaponHand = WeaponHand.MainHand;
            OnHandEquipmentChanged?.Invoke();
            return previous;
        }

        /// <summary>Переключает активную руку. Имеет смысл только при dual-wield (два одноручных оружия).</summary>
        public bool SwitchActiveWeapon()
        {
            if (!IsDualWielding)
            {
                Debug.Log("HandEquipment: переключать нечего — второе оружие не экипировано.");
                return false;
            }

            ActiveWeaponHand = ActiveWeaponHand == WeaponHand.MainHand ? WeaponHand.OffHand : WeaponHand.MainHand;
            Debug.Log($"HandEquipment: активная рука — {ActiveWeaponHand} ({ActiveWeapon?.itemName})");
            OnHandEquipmentChanged?.Invoke();
            return true;
        }
        /// <summary>
        /// Загрузка сохранения: заменяет руки целиком, ничего не возвращая в инвентарь
        /// (инвентарь восстанавливается своей секцией). Нарушающие правила предметы отбрасываются.
        /// </summary>
        public void RestoreHands(ItemInstance main, ItemInstance off, WeaponHand activeHand)
        {
            if (main != null && !(main.Data is WeaponData))
            {
                Debug.LogWarning($"HandEquipment: {main.Data.itemName} не оружие, MainHand пропущен.", this);
                main = null;
            }

            if (off != null && !IsValidOffHand(main, off))
            {
                Debug.LogWarning($"HandEquipment: {off.Data.itemName} нельзя держать во второй руке, OffHand пропущен.", this);
                off = null;
            }

            MainHandItem = main;
            OffHandItem = off;
            ActiveWeaponHand = activeHand == WeaponHand.OffHand && IsDualWielding ? WeaponHand.OffHand : WeaponHand.MainHand;
            OnHandEquipmentChanged?.Invoke();
        }

        private static bool IsValidOffHand(ItemInstance main, ItemInstance off)
        {
            if (main != null && main.Data is WeaponData mainWeapon && mainWeapon.isTwoHanded) return false;

            return off.Data is ShieldData || (off.Data is WeaponData offWeapon && !offWeapon.isTwoHanded);
        }
    }
}