using UnityEngine;
using Game.Combat;
using Game.Player.Equipment;
using Game.Player.Stats;
using Game.Stats;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Не часть архитектуры — удалить, когда появится реальный HUD / экран персонажа (Phase 14).
    /// Только читает данные, ничего не меняет. Показывает итоговые характеристики (после всех модификаторов),
    /// текущие HP/стамину/ману, нагрузку экипировки и сопротивления в правом верхнем углу, со скроллом.
    /// Окно сдвинуто влево на rightMargin, чтобы не перекрывать PlayerStateDebugOverlay (правый верхний угол).
    /// Скролл мышью: Ctrl включает курсор (DebugCursorToggle).
    /// </summary>
    public class StatsDebugOverlay : MonoBehaviour
    {
        [SerializeField] private CharacterStats characterStats;
        [SerializeField] private HealthComponent health;
        [SerializeField] private StaminaComponent stamina;
        [SerializeField] private ManaComponent mana;
        [Tooltip("Необязательно. Для веса и нагрузки экипировки.")]
        [SerializeField] private PlayerEquipment playerEquipment;
        [Tooltip("Необязательно. Для сопротивлений.")]
        [SerializeField] private DamageReceiver damageReceiver;
        [Tooltip("Стихии, сопротивление которым показывать (Fire, Dark, Light, Poison...). Учитывает бонусы от экипировки.")]
        [SerializeField] private ElementType[] shownElements;

        [Header("Окно")]
        [SerializeField] private float width = 300f;
        [SerializeField] private float height = 320f;
        [Tooltip("Отступ от правого края экрана. 340 — левее PlayerStateDebugOverlay.")]
        [SerializeField] private float rightMargin = 340f;
        [SerializeField] private float top = 10f;

        private Vector2 _scroll;

        private void OnGUI()
        {
            if (characterStats == null || characterStats.Stats == null) return;

            DerivedCharacterStats s = characterStats.Stats;

            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            label.normal.textColor = Color.white;
            GUIStyle header = new GUIStyle(label) { fontStyle = FontStyle.Bold };
            header.normal.textColor = new Color(1f, 0.85f, 0.4f);

            float x = Screen.width - width - rightMargin;
            GUI.Box(new Rect(x, top, width, height), "");
            GUILayout.BeginArea(new Rect(x + 8, top + 6, width - 16, height - 12));
            _scroll = GUILayout.BeginScrollView(_scroll, false, true);

            if (health != null) GUILayout.Label($"HP: {health.CurrentHealth} / {s.MaxHealth}", header);
            if (stamina != null) GUILayout.Label($"Stamina: {stamina.CurrentStamina:F0} / {s.MaxStamina:F0}", label);
            if (mana != null) GUILayout.Label($"Mana: {mana.CurrentMana:F0} / {s.MaxMana:F0}", label);

            GUILayout.Label($"Regen: {s.StaminaRegenPerSecond:F1}/s", header);
            if (stamina != null) GUILayout.Label($"Cost (итого с нагрузкой): x{stamina.CostMultiplier:F2}", label);

            GUILayout.Label($"Phys damage bonus: +{s.PhysicalDamageBonusPercent:F1}%", header);
            GUILayout.Label($"Magic damage: {s.MagicDamage:F1}", label);
            GUILayout.Label($"Defense: {s.DefenseValue:F1}", label);
            GUILayout.Label($"Cast speed: x{s.CastSpeedMultiplier:F2}", label);

            float loadRatio = playerEquipment != null ? playerEquipment.GetLoadRatio() : 0f;
            float moveMultiplier = EncumbranceService.GetMovementMultiplier(loadRatio);
            GUILayout.Label($"Speed: {s.MovementSpeed:F2} (итого {s.MovementSpeed * moveMultiplier:F2}, x{moveMultiplier:F2})", header);
            if (playerEquipment != null)
            {
                GUILayout.Label($"Weight: {playerEquipment.GetTotalWeight():F1} / {s.CarryWeightLimit:F1}  (load {loadRatio:F2})", label);
            }
            else
            {
                GUILayout.Label($"Carry limit: {s.CarryWeightLimit:F1}", label);
            }

            if (damageReceiver != null)
            {

                ResistanceProfileData profile = damageReceiver.Resistance;
                float physical = profile != null ? profile.GetPhysicalResistancePercent() : 0f;
                float magic = profile != null ? profile.GetMagicResistancePercent() : 0f;
                GUILayout.Label($"Physical: {physical:F0}%   Magic: {magic:F0}%", header);

                if (shownElements != null)
                {
                    foreach (ElementType element in shownElements)
                    {
                        if (element == null) continue;

                        string elementName = string.IsNullOrEmpty(element.displayName) ? element.name : element.displayName;
                        GUILayout.Label($"{elementName}: получает {damageReceiver.GetElementPercent(element):F0}% урона", label);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
