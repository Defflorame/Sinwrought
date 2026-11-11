using UnityEngine;
using Game.Combat;

namespace Game.Progression
{
    /// <summary>
    /// При смерти отдаёт награду
    /// RewardReceiver'у игрока. Только слушаем OnDeath.
    ///

    [RequireComponent(typeof(HealthComponent))]
    public class RewardDispenser : MonoBehaviour
    {
        [SerializeField] private RewardData reward;
        [SerializeField] private RewardReceiver receiver;

        private HealthComponent _health;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (receiver == null || reward == null)
            {
                Debug.LogWarning($"RewardDispenser: у {name} не назначен receiver или reward — награда не выдана.", this);
                return;
            }

            receiver.Grant(reward);
        }
        public void Configure(RewardData newReward, RewardReceiver newReceiver)
        {
            if (newReward != null) reward = newReward;
            if (newReceiver != null) receiver = newReceiver;
        }
    }
}
