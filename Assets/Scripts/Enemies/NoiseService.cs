using System;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Заглушка системы звука: любой источник шума вызывает Emit, слушатели (EnemyPerception)
    /// подписаны на событие. Громкость = радиус слышимости в метрах. Состояния не хранит.
    /// Настоящая звуковая система позже просто вызовет Emit.
    /// </summary>
    public static class NoiseService
    {
        /// <summary>position, loudness (радиус в метрах), source.</summary>
        public static event Action<Vector3, float, GameObject> OnNoise;

        public static void Emit(Vector3 position, float loudness, GameObject source)
        {
            if (loudness <= 0f) return;
            OnNoise?.Invoke(position, loudness, source);
        }
    }
}
