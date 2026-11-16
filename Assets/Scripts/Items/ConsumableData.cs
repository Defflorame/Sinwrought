using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>Расходуемый предмет (зелья и т.п.): что происходит при использовании. Стакается.</summary>
    [CreateAssetMenu(fileName = "NewConsumableData", menuName = "Game/Items/Consumable Data")]
    public class ConsumableData : ItemData
    {
        [SerializeField] private ConsumableEffectData[] effects;

        public IReadOnlyList<ConsumableEffectData> Effects => effects ?? Array.Empty<ConsumableEffectData>();

        // Значение для только что созданного ассета: расходники стакаются.
        private void Reset()
        {
            maxStackSize = 10;
        }
    }
}