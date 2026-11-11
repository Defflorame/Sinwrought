using System;
using UnityEngine;
using Game.Items;
using Game.Magic;

namespace Game.Enemies
{
    /// <summary>
    /// Одна атака врага. Либо ближняя (attack — AttackData, стихия и эффекты берутся из
    /// EnemyData.weapon), либо заклинание (spell). Указывается что-то одно.
    /// Данные, не runtime state: кулдауны хранит EnemyCombatController.
    /// </summary>
    [Serializable]
    public class EnemyAttackOption
    {
        public string label;

        [Tooltip("Ближняя атака. Оставить пустым, если это заклинание.")]
        public AttackData attack;

        [Tooltip("Заклинание (дальняя атака). Используются castTime (замах), эффекты и recoveryTime. Мана и кулдаун SpellData игнорируются.")]
        public SpellData spell;

        [Min(0f)] public float minRange = 0f;

        [Tooltip("0 = автоматически: длина хитбокса атаки × 0.9 (для заклинания — 15 м).")]
        [Min(0f)] public float maxRange = 0f;

        [Tooltip("Секунд до повторного использования ЭТОЙ атаки (считается от её начала).")]
        [Min(0f)] public float cooldown = 0f;

        [Tooltip("Вес при случайном выборе среди доступных атак.")]
        [Min(0.01f)] public float weight = 1f;
    }
}
