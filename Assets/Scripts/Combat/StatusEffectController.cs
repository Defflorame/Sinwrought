using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Stats;

namespace Game.Combat
{
    /// <summary>
    /// Накладывает временные эффекты, ведёт их таймеры и снимает по истечении времени.
    /// Модификаторы характеристик идут через CharacterStats, Source = экземпляр
    /// эффекта, поэтому снимаются только свои. Периодический урон отдаётся
    /// StatusEffectResolver → DamageResolver → DamageReceiver. Повторное наложение
    /// того же эффекта обновляет таймер (без стакания). Компонент выключен, пока
    /// активных эффектов нет. Подходит и игроку, и врагам: CharacterStats нужен только
    /// для эффектов с statModifiers, DamageReceiver — только для эффектов с тиками урона.
    /// </summary>
    public class StatusEffectController : MonoBehaviour
    {
        [Tooltip("Необязательно. Нужен эффектам с модификаторами характеристик (замедление, баффы).")]
        [SerializeField] private CharacterStats characterStats;
        [Tooltip("Необязательно. Нужен эффектам с периодическим уроном (яд, ожог, кровотечение).")]
        [SerializeField] private DamageReceiver damageReceiver;
        [Tooltip("Необязательно. Если назначено — все эффекты снимаются при смерти.")]
        [SerializeField] private HealthComponent health;

        private readonly List<StatusEffectInstance> _active = new List<StatusEffectInstance>();

        public IReadOnlyList<StatusEffectInstance> ActiveEffects => _active;

        public event Action<StatusEffectInstance> OnStatusApplied;
        public event Action<StatusEffectInstance> OnStatusExpired;

        private void Awake()
        {
            enabled = false;
            if (health != null) health.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDeath -= HandleDeath;
        }

        public bool CanApply => health == null || !health.IsDead();

        public void Apply(StatusEffectData data)
        {
            if (data == null || !CanApply) return;

            StatusEffectInstance existing = _active.Find(e => e.Data == data);
            if (existing != null)
            {
                existing.RemainingTime = data.duration;
                Debug.Log($"StatusEffectController: {data.name} обновлён ({data.duration:F1}s)");
                return;
            }

            var instance = new StatusEffectInstance(data);
            _active.Add(instance);

            ApplyModifiers(instance);

            if (data.HasPeriodicDamage && damageReceiver == null)
            {
                Debug.LogWarning($"StatusEffectController: у {data.name} есть периодический урон, но не назначен DamageReceiver — урона не будет.", this);
            }

            enabled = true;
            Debug.Log($"StatusEffectController: {data.name} наложен на {name} ({data.duration:F1}s)");
            OnStatusApplied?.Invoke(instance);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                // Смерть от тика очищает весь список прямо во время обхода.
                if (i >= _active.Count) continue;

                StatusEffectInstance instance = _active[i];

                TickPeriodicDamage(instance, dt);
                if (i >= _active.Count || _active[i] != instance) continue;

                instance.RemainingTime -= dt;
                if (instance.RemainingTime <= 0f)
                {
                    RemoveAt(i);
                }
            }

            if (_active.Count == 0)
            {
                enabled = false;
            }
        }

        private void TickPeriodicDamage(StatusEffectInstance instance, float dt)
        {
            StatusEffectData data = instance.Data;
            if (!data.HasPeriodicDamage || damageReceiver == null) return;

            instance.TimeToNextTick -= dt;
            if (instance.TimeToNextTick > 0f) return;

            instance.TimeToNextTick = Mathf.Max(0.01f, instance.TimeToNextTick + data.tickInterval);
            StatusEffectResolver.ResolveTick(instance, damageReceiver);
        }

        private void ApplyModifiers(StatusEffectInstance instance)
        {
            StatusEffectData data = instance.Data;
            if (data.statModifiers == null || data.statModifiers.Length == 0) return;

            if (characterStats == null)
            {
                Debug.LogWarning($"StatusEffectController: у {data.name} есть модификаторы характеристик, но у {name} нет CharacterStats — они не применены.", this);
                return;
            }

            foreach (var definition in data.statModifiers)
            {
                characterStats.AddModifier(new StatModifier(definition.stat, definition.type, definition.value, instance));
            }
        }

        /// <summary>Снимает все эффекты (смерть, загрузка сохранения). Модификаторы характеристик снимаются.</summary>
        public void ClearAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }

            enabled = false;
        }

        private void HandleDeath()
        {
            ClearAll();
        }

        private void RemoveAt(int index)
        {
            StatusEffectInstance instance = _active[index];
            _active.RemoveAt(index);

            if (characterStats != null)
            {
                characterStats.RemoveModifiersFromSource(instance);
            }

            Debug.Log($"StatusEffectController: {instance.Data.name} закончился");
            OnStatusExpired?.Invoke(instance);
        }
    }
}
