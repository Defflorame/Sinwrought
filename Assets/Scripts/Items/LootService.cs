using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>Результат броска одной записи таблицы: что выпало и сколько.</summary>
    public readonly struct LootDrop
    {
        public readonly ItemData Item;
        public readonly int Quantity;

        public LootDrop(ItemData item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// Бросок таблицы дропа. Единственное место, где разыгрывается дроп: Roll возвращает результат,
    /// а что с ним делать — решает вызывающий (сундук кладёт в инвентарь через Grant, враг выбрасывает мешки).
    /// </summary>
    public static class LootService
    {
        /// <summary>Бросает каждую запись отдельно. Пустой список — ничего не выпало.</summary>
        public static List<LootDrop> Roll(DropTableData table)
        {
            var result = new List<LootDrop>();
            if (table == null) return result;

            foreach (DropEntry entry in table.Entries)
            {
                if (entry == null || entry.item == null) continue;

                // Random.value включает 1.0, поэтому 100% обрабатываем отдельно.
                bool dropped = entry.chancePercent >= 100f || Random.value * 100f < entry.chancePercent;
                if (!dropped) continue;

                int quantity = Random.Range(entry.min, entry.max + 1);
                result.Add(new LootDrop(entry.item, quantity));
            }

            return result;
        }

        /// <summary>Бросает таблицу и кладёт выпавшее прямо в инвентарь (сундуки).</summary>
        public static void Grant(DropTableData table, Inventory inventory)
        {
            if (inventory == null) return;

            foreach (LootDrop drop in Roll(table))
            {
                inventory.AddItem(new ItemInstance(drop.Item), drop.Quantity);
                Debug.Log($"LootService: получено {drop.Item.name} x{drop.Quantity}");
            }
        }
    }
}