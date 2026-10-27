using UnityEngine;
using Game.Combat;

namespace Game.Progression
{
    /// <summary>
    /// Висит на убиваемом объекте (манекен, позже враг). При смерти отдаёт награду
    /// RewardReceiver'у игрока. Combat про Progression ничего не знает: мы только слушаем OnDeath.
    ///
    /// TODO Phase 8: у врагов, создаваемых в рантайме, ссылку на RewardReceiver нельзя назначить
    /// в инспекторе префаба — её передаст спавнер/EnemyController при создании врага.
    /// </summary>
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
    }
}
