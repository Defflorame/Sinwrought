using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>Одна запись таблицы дропа: предмет, диапазон количества и независимый шанс выпадения.</summary>
    [Serializable]
    public class DropEntry
    {
        public ItemData item;

        [Min(1)] public int min = 1;
        [Min(1)] public int max = 1;

        [Range(0f, 100f)]
        [Tooltip("Шанс выпадения этой записи, %. Каждая запись бросается отдельно.")]
        public float chancePercent = 100f;
    }

    /// <summary>
    /// Таблица дропа (данные). Один ассет подходит и для сундуков, и для врагов.
    /// Runtime-состояния не хранит: бросок делает LootService.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDropTable", menuName = "Game/Items/Drop Table Data")]
    public class DropTableData : ScriptableObject
    {
        [SerializeField] private DropEntry[] entries;

        public IReadOnlyList<DropEntry> Entries => entries ?? Array.Empty<DropEntry>();

        private void OnValidate()
        {
            if (entries == null) return;

            foreach (DropEntry entry in entries)
            {
                if (entry != null) entry.max = Mathf.Max(entry.min, entry.max);
            }
        }
    }
}