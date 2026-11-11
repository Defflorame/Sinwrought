using UnityEngine;
using Game.Items;

namespace Game.Combat
{
    /// <summary>
    /// Incoming Damage + Shield Efficiency + Attacker Power + Player Modifiers
    /// → Blocked Damage (раздел 31 документа). Эффективность щита берётся из
    /// ShieldData отдельно для физического и магического урона: щит может хорошо
    /// держать меч и плохо — заклинания (или наоборот).
    /// </summary>
    public class BlockResolver : MonoBehaviour
    {
        [Tooltip("Урон атаки, при котором эффективность щита падает до нуля (чем сильнее удар — тем хуже блокируется).")]
        [SerializeField] private float attackerPowerSoftCap = 100f;

        [Header("Stamina")]
        [SerializeField] private StaminaComponent stamina;
        [Tooltip("Сколько стамины стоит каждая единица урона, поглощённого щитом.")]
        [SerializeField] private float staminaPerAbsorbedDamage = 0.5f;
        private float _physicalEfficiency;
        private float _magicEfficiency;
        private float _staggerMultiplier = 1f;
        public bool IsBlocking { get; private set; }

        /// <summary>shield может быть null только если блок был начат без щита — вызывающий код (PlayerCombatController) не должен этого допускать.</summary>
        public void StartBlock(ShieldData shield)
        {
            _physicalEfficiency = shield != null ? shield.blockEfficiency : 0f;
            _magicEfficiency = shield != null ? shield.magicBlockEfficiency : 0f;
            _staggerMultiplier = shield != null ? shield.blockStaggerMultiplier : 1f;
            IsBlocking = true;
            Debug.Log($"BlockResolver: блок начат (physical {_physicalEfficiency:F2}, magic {_magicEfficiency:F2})");
        }

        public void StopBlock()
        {
            IsBlocking = false;
            Debug.Log("BlockResolver: блок закончен");
        }

        public int ResolveBlockedDamage(int baseDamage, DamageKind kind, out float staggerMultiplier)
        {
            staggerMultiplier = _staggerMultiplier;
            if (baseDamage <= 0) return 0;

            float shieldEfficiency = kind == DamageKind.Magic ? _magicEfficiency : _physicalEfficiency;

            float attackerPowerPenalty = Mathf.Clamp01(baseDamage / attackerPowerSoftCap) * shieldEfficiency;
            float effectiveEfficiency = Mathf.Clamp01(shieldEfficiency - attackerPowerPenalty);

            int damageAfterBlock = Mathf.Max(0, Mathf.RoundToInt(baseDamage * (1f - effectiveEfficiency)));
            int absorbed = baseDamage - damageAfterBlock;

            if (stamina != null && !stamina.TryConsume(absorbed * staminaPerAbsorbedDamage))
            {
                Debug.Log("BlockResolver: не хватило stamina — гард пробит, урон проходит полностью");
                staggerMultiplier = 1f;
                return baseDamage;
            }

            return damageAfterBlock;
        }
    }
}
