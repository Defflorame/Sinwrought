using UnityEngine;
using Game.Stats;

namespace Game.Magic
{
    /// <summary>
    /// Собирает SpellCastContext и выполняет эффекты заклинания по порядку.
    /// О конкретных типах эффектов не знает — новые эффекты добавляются
    /// созданием нового SpellEffectData, а не правкой этого класса.
    /// Висит на КОРНЕ объекта-заклинателя: его gameObject — источник урона,
    /// а коллайдеры всех его детей считаются «своими» и не поражаются.
    /// </summary>
    public class SpellEffectExecutor : MonoBehaviour
    {
        [Tooltip("Откуда вылетают эффекты и куда смотрит прицел — обычно камера игрока.")]
        [SerializeField] private Transform castOrigin;
        [SerializeField] private CharacterStats characterStats;
        [Tooltip("Слои, с которыми взаимодействуют заклинания этого заклинателя: прицел (хитскан, зоны), попадание снаряда и область взрыва. Должны входить окружение (Default) и враждебная сторона. Игрок: Default + Enemy. Враг: Default + Player.")]
        [SerializeField] private LayerMask aimMask = ~0;

        /// <summary>
        /// Выпускает все эффекты. false — выпускать нечего или хотя бы один эффект не может сработать:
        /// тогда не выполняется НИ ОДИН эффект и SpellCaster ничего не списывает.
        /// </summary>
        public bool Execute(SpellData spell)
        {
            if (spell.effects == null || spell.effects.Length == 0)
            {
                Debug.LogWarning($"SpellEffectExecutor: у {spell.name} не назначено ни одного эффекта.", this);
                return false;
            }

            if (castOrigin == null)
            {
                Debug.LogWarning("SpellEffectExecutor: не назначен castOrigin.", this);
                return false;
            }

            float magicDamage = characterStats != null ? characterStats.Stats.MagicDamage : 0f;
            var context = new SpellCastContext(spell, gameObject, castOrigin, magicDamage, aimMask);

            foreach (SpellEffectData effect in spell.effects)
            {
                if (effect != null && !effect.CanExecute(context))
                {
                    Debug.Log($"SpellEffectExecutor: эффект {effect.name} не может сработать — каст {spell.name} сорван без трат");
                    return false;
                }
            }

            foreach (SpellEffectData effect in spell.effects)
            {
                if (effect != null) effect.Execute(context);
            }

            return true;
        }
    }
}
