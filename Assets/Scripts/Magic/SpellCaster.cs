using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Combat;
using Game.Stats;

namespace Game.Magic
{
    /// <summary>
    /// Проверяет и списывает стоимость заклинания, ведёт кулдауны и сообщает о
    /// стадиях каста событиями (для будущих PlayerAnimator/Audio/VFX/HUD).
    /// Не знает про кольца, ввод и состояния: время заряда считает CastState,
    /// но длительности (с учётом характеристики CastSpeed) берёт отсюда.
    /// Стоимость списывается и кулдаун стартует ТОЛЬКО в TryRelease.
    /// </summary>
    public class SpellCaster : MonoBehaviour
    {
        [SerializeField] private ManaComponent mana;
        [SerializeField] private HealthComponent health;
        [SerializeField] private SpentHealthRecovery spentHealthRecovery;
        [SerializeField] private SpellEffectExecutor effectExecutor;
        [Tooltip("Необязательно. Если назначено — время каста и восстановления делится на характеристику CastSpeed.")]
        [SerializeField] private CharacterStats characterStats;

        public event Action<SpellData> OnChargeStarted;
        public event Action<SpellData> OnChargeReady;
        public event Action<SpellData> OnChargeCancelled;
        public event Action<SpellData> OnSpellReleased;

        private readonly Dictionary<SpellData, float> _readyAt = new Dictionary<SpellData, float>();

        private float CastSpeedMultiplier =>
            characterStats != null && characterStats.Stats != null ? characterStats.Stats.CastSpeedMultiplier : 1f;

        /// <summary>Сколько нужно держать кнопку до готовности заряда (castTime / CastSpeed).</summary>
        public float GetCastTime(SpellData spell) => spell.castTime / CastSpeedMultiplier;

        /// <summary>Сколько после выпуска нельзя действовать (recoveryTime / CastSpeed).</summary>
        public float GetRecoveryTime(SpellData spell) => spell.recoveryTime / CastSpeedMultiplier;

        public float GetCooldownRemaining(SpellData spell)
        {
            if (spell == null || !_readyAt.TryGetValue(spell, out float readyAt)) return 0f;
            return Mathf.Max(0f, readyAt - Time.time);
        }

        /// <summary>Проверка при НАЖАТИИ: жив ли заклинатель, кулдаун, хватает ли ресурса. Ничего не списывает.</summary>
        public bool CanCast(SpellData spell)
        {
            if (!IsCasterAlive(spell)) return false;

            if (GetCooldownRemaining(spell) > 0f)
            {
                Debug.Log($"SpellCaster: {spell.name} на кулдауне");
                return false;
            }

            return HasResources(spell);
        }

        public void StartCharge(SpellData spell)
        {
            OnChargeStarted?.Invoke(spell);
        }

        public void ReportChargeReady(SpellData spell)
        {
            OnChargeReady?.Invoke(spell);
        }

        public void CancelCharge(SpellData spell)
        {
            Debug.Log($"SpellCaster: заряд {spell.name} отменён без трат");
            OnChargeCancelled?.Invoke(spell);
        }

        /// <summary>
        /// Выпуск: повторная проверка ресурса (за время заряда он мог измениться),
        /// эффекты, списание стоимости, кулдаун. false — ничего не потрачено
        /// (не хватает ресурса или эффекты не могут сработать: нет точки прицеливания и т.п.).
        /// </summary>
        public bool TryRelease(SpellData spell)
        {
            if (!IsCasterAlive(spell) || !HasResources(spell)) return false;

            if (effectExecutor == null)
            {
                Debug.LogWarning("SpellCaster: не назначен SpellEffectExecutor.", this);
                return false;
            }

            // Эффекты выпускаются до списания: если выпускать нечего — ничего не тратим.
            if (!effectExecutor.Execute(spell)) return false;

            PayCost(spell);
            _readyAt[spell] = Time.time + spell.cooldown;
            OnSpellReleased?.Invoke(spell);
            return true;
        }

        private bool IsCasterAlive(SpellData spell)
        {
            return spell != null && health != null && !health.IsDead();
        }

        private bool HasResources(SpellData spell)
        {
            switch (spell.resource)
            {
                case SpellResource.Mana:
                    if (mana == null)
                    {
                        Debug.LogWarning("SpellCaster: не назначен ManaComponent.", this);
                        return false;
                    }

                    if (!mana.CanSpend(spell.cost))
                    {
                        Debug.Log($"SpellCaster: недостаточно маны для {spell.name}");
                        return false;
                    }
                    return true;

                case SpellResource.Health:
                    // Заклинание за HP не может убить самого заклинателя.
                    if (health.CurrentHealth <= spell.cost)
                    {
                        Debug.Log($"SpellCaster: недостаточно HP для {spell.name}");
                        return false;
                    }
                    return true;
            }

            return false;
        }

        private void PayCost(SpellData spell)
        {
            if (spell.resource == SpellResource.Mana)
            {
                mana.TryConsume(spell.cost);
                return;
            }

            health.SpendHealth(spell.cost);
            if (spell.healthCostRecovers && spentHealthRecovery != null)
            {
                spentHealthRecovery.AddPending(spell.cost);
            }
        }
    }
}
