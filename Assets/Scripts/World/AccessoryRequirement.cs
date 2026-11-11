using UnityEngine;
using Game.Items;
using Game.Player.Equipment;

namespace Game.World
{
    /// <summary>Нужен надетый аксессуар (например, маска от газа). Ничего не расходует.</summary>
    [CreateAssetMenu(fileName = "NewAccessoryRequirement", menuName = "Game/World/Requirements/Accessory")]
    public class AccessoryRequirement : AccessRequirement
    {
        [SerializeField] private AccessoryData accessory;

        public override bool IsMet(GameObject owner)
        {
            if (accessory == null || owner == null) return false;

            PlayerEquipment equipment = owner.GetComponentInChildren<PlayerEquipment>();
            ItemInstance equipped = equipment != null ? equipment.GetEquipped(EquipmentSlot.Accessory) : null;
            return equipped != null && equipped.Data == accessory;
        }
    }
}