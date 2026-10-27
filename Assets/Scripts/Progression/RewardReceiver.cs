using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Висит на игроке. Единственное место, которое знает про оба кошелька:
    /// раскладывает награду по ним. Позже здесь же будет выдача предметов в Inventory.
    /// </summary>
    public class RewardReceiver : MonoBehaviour
    {
        [SerializeField] private ProgressionResourceWallet resourceWallet;
        [SerializeField] private CurrencyWallet currencyWallet;

        public void Grant(RewardData reward)
        {
            if (reward == null) return;

            if (resourceWallet != null) resourceWallet.Add(reward.experience);
            if (currencyWallet != null) currencyWallet.Add(reward.currency);
        }
    }
}
