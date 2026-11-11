using UnityEngine;
using UnityEngine.AI;
using Game.Combat;
using Game.Progression;
using Game.Stats;

namespace Game.Enemies
{
    /// <summary>
    /// Координатор врага: собирает компоненты, применяет к ним EnemyData и запускает EnemyBrain.
    /// Не содержит логики поведения, боя или зрения.
    /// Все компоненты — на одном корневом объекте. Враг, созданный из кода, должен получить
    /// Initialize(target, rewardReceiver) сразу после Instantiate (до первого кадра).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(DamageReceiver))]
    [RequireComponent(typeof(EnemyStateMachine))]
    [RequireComponent(typeof(EnemyBrain))]
    [RequireComponent(typeof(EnemyPerception))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyCombatController))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyData data;

        [Header("Этот экземпляр на сцене")]
        [Tooltip("Цель врага (корень игрока). Для врагов из спавнера задаётся через Initialize.")]
        [SerializeField] private Transform target;
        [Tooltip("Кому выдавать награду за убийство (RewardReceiver игрока). Пусто — награда не выдаётся.")]
        [SerializeField] private RewardReceiver rewardReceiver;
        [SerializeField] private EnemyPursuitMode pursuitMode = EnemyPursuitMode.Normal;
        [Tooltip("Точки патруля. Если заданы — враг ходит по ним, иначе по случайным точкам в patrolRadius.")]
        [SerializeField] private Transform[] patrolWaypoints;
        [Tooltip("Ходить по точкам туда-обратно, а не по кругу.")]
        [SerializeField] private bool pingPongPatrol;

        private EnemyBrain _brain;
        private EnemyStateMachine _stateMachine;
        private EnemyPerception _perception;
        private EnemyMovement _movement;
        private EnemyCombatController _combat;
        private CharacterStats _characterStats;
        private DamageReceiver _damageReceiver;
        private HealthComponent _health;
        private RewardDispenser _rewardDispenser;

        public EnemyData Data => data;

        private void Awake()
        {
            if (data == null)
            {
                Debug.LogError($"EnemyController: у {name} не назначен EnemyData.", this);
                enabled = false;
                return;
            }

            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true; // нужен только чтобы по врагу срабатывали триггеры оружия и заклинаний
            body.useGravity = false;

            _brain = GetComponent<EnemyBrain>();
            _stateMachine = GetComponent<EnemyStateMachine>();
            _perception = GetComponent<EnemyPerception>();
            _movement = GetComponent<EnemyMovement>();
            _combat = GetComponent<EnemyCombatController>();
            _characterStats = GetComponent<CharacterStats>();
            _damageReceiver = GetComponent<DamageReceiver>();
            _health = GetComponent<HealthComponent>();
            _rewardDispenser = GetComponent<RewardDispenser>();
            // EnemyData главнее значения в CharacterStats на префабе: то нужно лишь чтобы Awake CharacterStats не ругался.
            if (data.baseStats != null) _characterStats.SetBaseStats(data.baseStats);
            if (data.resistanceProfile != null) _damageReceiver.SetResistanceProfile(data.resistanceProfile);

            _perception.Configure(data);
            _movement.Configure(data);
            _combat.Configure(data);

            ConfigureReward();
        }

        private void Start()
        {
            if (data == null) return;

            _perception.SetTarget(target);

            var context = new EnemyContext
            {
                Data = data,
                Self = transform,
                Target = target,
                TargetHealth = target != null ? target.GetComponentInParent<HealthComponent>() : null,
                HomePosition = transform.position,
                Pursuit = pursuitMode,
                Waypoints = patrolWaypoints,
                PingPongPatrol = pingPongPatrol,
                Movement = _movement,
                Perception = _perception,
                Combat = _combat,
                Receiver = _damageReceiver,
                Health = _health,
                Stats = _characterStats 
            };

            _brain.Begin(context, _stateMachine);
        }

        /// <summary>Для врагов, созданных из кода: вызвать сразу после Instantiate.</summary>
        public void Initialize(Transform newTarget, RewardReceiver newRewardReceiver)
        {
            target = newTarget;
            rewardReceiver = newRewardReceiver;

            if (_rewardDispenser != null) ConfigureReward();
        }

        private void ConfigureReward()
        {
            if (_rewardDispenser != null)
            {
                _rewardDispenser.Configure(data.reward, rewardReceiver);
            }
        }
    }
}
