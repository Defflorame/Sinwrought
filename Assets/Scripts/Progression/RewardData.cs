using UnityEngine;
using UnityEngine.Serialization;
using Game.Items;

namespace Game.Progression
{
    /// <summary>
    /// Награда за убийство. Один ассет можно назначить многим врагам.
    /// Опыт и валюта выдаются сразу (случайно в диапазоне), предметы выпадают на пол мешками.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRewardData", menuName = "Game/Progression/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Header("Experience")]
        [FormerlySerializedAs("experience")]
        [Min(0)] public int experienceMin = 10;
        [Min(0)] public int experienceMax = 10;

        [Header("Currency")]
        [FormerlySerializedAs("currency")]
        [Min(0)] public int currencyMin = 5;
        [Min(0)] public int currencyMax = 5;

        [Header("Drops")]
        [Tooltip("Предметы, которые выпадут мешками на пол. Пусто — предметов нет.")]
        public DropTableData drops;

        private void OnValidate()
        {
            experienceMax = Mathf.Max(experienceMin, experienceMax);
            currencyMax = Mathf.Max(currencyMin, currencyMax);
        }
    }
}