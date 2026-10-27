using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Единая система расчёта БАЗОВОГО урона для игрока и врагов.
    /// Физическая часть: Base Damage × (1 + Physical Damage Bonus атакующего) × (1 − physicalResistance/100) цели
    /// Стихийная часть: см. ElementalDamageResolver (magicResistance для магии × множитель стихии).
    /// Итог: (физическая + стихийная) − Defense цели, округление один раз в конце.
    /// Block и парирование — в DamageReceiver.
    /// </summary>
    public static class DamageResolver
    {
        public static void ResolveDamage(DamagePacket packet, DamageReceiver receiver)
        {
            if (receiver.Health != null && receiver.Health.IsDead()) return;
            int baseFinalDamage = CalculateFinalDamage(packet, receiver);
            receiver.ReceiveDamage(packet, baseFinalDamage);
        }

        private static int CalculateFinalDamage(DamagePacket packet, DamageReceiver receiver)
        {
            float physical = CalculatePhysicalPart(packet, receiver);
            float elemental = ElementalDamageResolver.Calculate(packet, receiver);

            float defense = packet.IgnoreDefense || receiver.CharacterStats == null
            ? 0f
            : receiver.CharacterStats.Stats.DefenseValue;

            float finalDamage = Mathf.Max(0f, physical + elemental - defense);

            string note = finalDamage <= 0f && (physical + elemental) > 0f ? "  ← весь урон поглощён защитой" : "";
            Debug.Log($"DamageResolver: {receiver.name} — физ. {physical:F1} + стихия {elemental:F1} − защита {defense:F1} = {finalDamage:F1}{note}");
            return Mathf.RoundToInt(finalDamage);
        }

        private static float CalculatePhysicalPart(DamagePacket packet, DamageReceiver receiver)
        {
            if (packet.BasePhysicalDamage <= 0) return 0f;

            float damage = packet.BasePhysicalDamage * packet.DamageMultiplier;

            float bonusPercent = packet.AttackerStats != null
                ? packet.AttackerStats.Stats.PhysicalDamageBonusPercent
                : 0f;
            damage *= 1f + bonusPercent / 100f;

            if (receiver.Resistance != null)
            {
                damage *= 1f - receiver.Resistance.GetPhysicalResistancePercent() / 100f;
            }

            return damage;
        }
    }
}
