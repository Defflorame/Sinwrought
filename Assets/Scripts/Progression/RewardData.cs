using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Награда за убийство. Один ассет можно назначить многим врагам.
    /// Позже сюда добавится ссылка на LootTableData (дроп предметов) — RewardReceiver
    /// и RewardDispenser при этом не изменятся.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRewardData", menuName = "Game/Progression/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Min(0)] public int experience = 10;
        [Min(0)] public int currency = 5;
    }
}
