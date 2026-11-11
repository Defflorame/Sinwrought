using UnityEngine;
using Game.Combat;
using Game.Stats;

namespace Game.Player
{
    /// <summary>
    /// Связывает полученный удар с оглушением игрока: сила удара → PoiseTracker → сломан → TryStagger.
    /// Poise-пул тот же класс, что у врагов. Вешается на корень игрока.
    /// Если poise сломался во время отскока (Dodge), оглушения нет, а запас уже восстановлен.
    /// </summary>
    public class PlayerStaggerController : MonoBehaviour
    {
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private CharacterStats characterStats;
        [SerializeField] private PlayerCombatController combatController;

        [Min(0f)]
        [Tooltip("Через сколько секунд без попаданий запас poise восстанавливается полностью.")]
        [SerializeField] private float poiseResetDelay = 3f;

        [Min(0f)]
        [Tooltip("Сколько секунд игрок оглушён после сломанного poise: не может атаковать, блокировать, колдовать и ходить.")]
        [SerializeField] private float staggerDuration = 0.5f;

        private PoiseTracker _poise;
        public PoiseTracker Poise => _poise;

        private void Awake()
        {
            _poise = new PoiseTracker(characterStats, poiseResetDelay);
        }

        private void OnEnable()
        {
            damageReceiver.OnStaggerReceived += HandleStaggerReceived;
        }

        private void OnDisable()
        {
            damageReceiver.OnStaggerReceived -= HandleStaggerReceived;
        }

        private void HandleStaggerReceived(float staggerPower)
        {
            if (!_poise.Absorb(staggerPower)) return;

            combatController.TryStagger(staggerDuration);
        }
    }
}