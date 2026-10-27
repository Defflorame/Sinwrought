using UnityEngine;
using Game.Stats;
using System.Collections.Generic;

namespace Game.Combat
{
    /// <summary>
    /// Общий компонент получения урона. Используется и игроком, и врагами.
    /// BlockResolver/ParryResolver/CharacterStats/ResistanceProfileData/StatusEffectController
    /// необязательны (могут быть null) — назначаются только тем объектам, у которых они есть.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class DamageReceiver : MonoBehaviour
    {
        [SerializeField] private BlockResolver blockResolver;
        [SerializeField] private ParryResolver parryResolver;
        [SerializeField] private CharacterStats characterStats;
        [Tooltip("Необязательно. Пассивные сопротивления юнита (физика, магия, стихии, статус-эффекты).")]
        [SerializeField] private ResistanceProfileData resistanceProfile;
        [Tooltip("Необязательно. Без него юнит не получает статус-эффекты от ударов и заклинаний.")]
        [SerializeField] private StatusEffectController statusEffects;
        [Tooltip("Необязательно. Бонусы к сопротивлению стихиям от экипировки/эффектов.")]
        [SerializeField] private ElementResistanceModifiers resistanceModifiers;

        private HealthComponent _health;

        public CharacterStats CharacterStats => characterStats;
        public ResistanceProfileData Resistance => resistanceProfile;
        public HealthComponent Health => _health;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        /// <summary>Подменяет профиль сопротивлений (профиль выбранного класса игрока).</summary>
        public void SetResistanceProfile(ResistanceProfileData profile)
        {
            resistanceProfile = profile;
        }

        /// <summary>
        /// baseDamage — уже посчитанный DamageResolver базовый урон,
        /// ДО применения блока/парирования.
        /// </summary>
        public void ReceiveDamage(DamagePacket packet, int baseDamage)
        {
            if (_health.IsDead()) return;
            if (parryResolver != null && parryResolver.TryResolveIncomingAttack(packet))
            {
                Debug.Log($"{name}: атака спарирована и урон не получен.");
                return;
            }

            int finalDamage = baseDamage;

            if (packet.IsBlockable && blockResolver != null && blockResolver.IsBlocking)
            {
                finalDamage = blockResolver.ResolveBlockedDamage(baseDamage, packet.Kind);
                Debug.Log($"{name}: блокировал часть урона ({packet.Kind}). Должно {baseDamage} -> нанесено {finalDamage}");
            }

            if (finalDamage > 0)
            {
                _health.TakeDamage(finalDamage);
                // Эффекты накладываются, только если хоть какой-то урон прошёл:
                // полностью заблокированный или спарированный удар их не вешает.
                StatusEffectResolver.ResolveOnHit(packet.OnHitEffects, statusEffects, resistanceProfile);
            }

            if (packet.AttackData != null && packet.AttackData.staggerPower > 0f)
            {
                ReceiveStagger(packet.AttackData.staggerPower);
            }
        }

        /// <summary>
        /// Стаггер от полученного удара. Реальная игровая реакция
        /// (прерывание анимации, StaggerState) появится вместе
        /// с Enemy AI (Phase 8) / Player Stagger (по необходимости).
        /// </summary>
        public void ReceiveStagger(float staggerPower)
        {
            // Debug.Log($"{name}: received stagger {staggerPower:F1} (пока без игровой реакции)");
        }

        /// <summary>Вызывается, когда именно ЭТОТ объект был атакующим и его атаку парировали.</summary>
        public void ReceiveParry()
        {
            Debug.Log($"{name}: ReceiveParry() — атака этого объекта была парирована.");
        }

        public void ReceiveKnockback(Vector3 direction, float force)
        {
            Debug.Log($"{name}: ReceiveKnockback({direction}, {force:F1}) — пока без физической реакции.");
        }

        /// <summary>
        /// Итоговый % стихийного урона: значение профиля минус бонус сопротивления
        /// от экипировки/эффектов (в процентных пунктах). Зажим 0..200.
        /// 100 — норма, 0 — иммунитет, 200 — двойной урон.
        /// </summary>
        public float GetElementPercent(ElementType element)
        {
            float percent = resistanceProfile != null ? resistanceProfile.GetElementPercent(element) : 100f;
            if (element == null || resistanceModifiers == null) return percent;

            return Mathf.Clamp(percent - resistanceModifiers.GetBonusPercent(element), 0f, 200f);
        }
    }
}
