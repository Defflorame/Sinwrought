using UnityEngine;
using Game.Items;

namespace Game.World
{
    /// <summary>Нужен предмет в инвентаре (ключ). Использование забирает один экземпляр.</summary>
    [CreateAssetMenu(fileName = "NewItemRequirement", menuName = "Game/World/Requirements/Item")]
    public class ItemRequirement : AccessRequirement
    {
        [SerializeField] private ItemData item;

        public override bool IsMet(GameObject owner)
        {
            if (item == null || owner == null) return false;

            Inventory inventory = owner.GetComponentInChildren<Inventory>();
            return inventory != null && inventory.HasItem(item);
        }

        public override void Consume(GameObject owner)
        {
            Inventory inventory = owner.GetComponentInChildren<Inventory>();
            ItemStack stack = inventory != null ? inventory.FindItem(item) : null;
            if (stack != null) inventory.RemoveItem(stack.Item);
        }
    }
}