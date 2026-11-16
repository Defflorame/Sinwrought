using UnityEngine;
using Game.Combat;
using Game.Items;
using Game.World;

namespace Game.Progression
{
    /// <summary>
    /// Висит на убиваемом объекте. При смерти отдаёт опыт и валюту RewardReceiver'у игрока
    /// и просит LootDropper выбросить предметы на пол. Combat про Progression ничего не знает:
    /// мы только слушаем OnDeath.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class RewardDispenser : MonoBehaviour
    {
        [SerializeField] private RewardData reward;
        [SerializeField] private RewardReceiver receiver;

        [Tooltip("Необязательно. Пусто — берётся LootDropper с этого же объекта; нет его — предметы не выпадают.")]
        [SerializeField] private LootDropper lootDropper;

        private HealthComponent _health;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            if (lootDropper == null) lootDropper = GetComponent<LootDropper>();
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
            if (reward == null)
            {
                Debug.LogWarning($"RewardDispenser: у {name} не назначен reward — награда не выдана.", this);
                return;
            }

            if (receiver != null) receiver.Grant(reward);
            else Debug.LogWarning($"RewardDispenser: у {name} не назначен receiver — опыт и валюта не выданы.", this);

            if (lootDropper != null) lootDropper.Drop(LootService.Roll(reward.drops));
        }

        public void Configure(RewardData newReward, RewardReceiver newReceiver)
        {
            if (newReward != null) reward = newReward;
            if (newReceiver != null) receiver = newReceiver;
        }
    }
}