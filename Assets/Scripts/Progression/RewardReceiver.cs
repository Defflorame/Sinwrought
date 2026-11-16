using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Висит на игроке. Единственное место, которое знает про оба кошелька:
    /// бросает диапазоны награды и раскладывает опыт и валюту по ним.
    /// Предметы сюда не приходят: они выпадают в мир (LootDropper).
    /// </summary>
    public class RewardReceiver : MonoBehaviour
    {
        [SerializeField] private ProgressionResourceWallet resourceWallet;
        [SerializeField] private CurrencyWallet currencyWallet;

        public void Grant(RewardData reward)
        {
            if (reward == null) return;

            if (resourceWallet != null) resourceWallet.Add(RollRange(reward.experienceMin, reward.experienceMax));
            if (currencyWallet != null) currencyWallet.Add(RollRange(reward.currencyMin, reward.currencyMax));
        }

        // Max защищаем от старых ассетов, у которых новое поле ещё не заполнено.
        private static int RollRange(int min, int max) => Random.Range(min, Mathf.Max(min, max) + 1);
    }
}