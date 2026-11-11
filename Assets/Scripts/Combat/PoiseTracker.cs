using UnityEngine;
using Game.Stats;

namespace Game.Combat
{
    /// <summary>
    /// Runtime-запас устойчивости к оглушению. Общий для игрока и врагов.
    /// Максимум — характеристика Poise из CharacterStats (броня, кольца, способности).
    /// Каждый удар отнимает staggerPower; запас кончился — Absorb вернёт true (оглушить)
    /// и запас сразу восстановится. Без ударов resetDelay секунд — восстановление полное.
    /// Update не нужен: восстановление считается лениво по времени последнего удара.
    /// </summary>
    public class PoiseTracker
    {
        private readonly CharacterStats _stats;
        private readonly float _resetDelay;
        private float _current;
        private float _lastHitTime = float.NegativeInfinity;

        public PoiseTracker(CharacterStats stats, float resetDelay)
        {
            _stats = stats;
            _resetDelay = resetDelay;
        }

        public float Max => _stats.Stats.MaxPoise;

        /// <summary>Текущий запас с учётом восстановления по времени (для отладки и UI).</summary>
        public float Current => Time.time - _lastHitTime > _resetDelay ? Max : Mathf.Min(_current, Max);

        /// <summary>
        /// Принять удар. powerDivisor делит силу удара (враг во время своей атаки). true — запас пробит.
        /// </summary>
        public bool Absorb(float staggerPower, float powerDivisor = 1f)
        {
            if (staggerPower <= 0f) return false;

            float max = Max;
            _current = Time.time - _lastHitTime > _resetDelay ? max : Mathf.Min(_current, max); // броню могли снять

            _lastHitTime = Time.time;
            _current -= staggerPower / Mathf.Max(1f, powerDivisor);

            if (_current > 0f) return false;

            _current = max;
            return true;
        }
    }
}