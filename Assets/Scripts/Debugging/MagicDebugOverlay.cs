using UnityEngine;
using UnityEngine.InputSystem;
using Game.Combat;
using Game.Magic;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Удалить, когда появится HUD (Phase 14).
    /// Показывает активное кольцо/заклинание, кулдаун, ману, HP, заряд каста
    /// и активные статус-эффекты. Заряд берётся из событий SpellCaster, а не опросом состояний.
    /// Клавиша M — восполнить ману (предметов маны пока нет).
    /// </summary>
    public class MagicDebugOverlay : MonoBehaviour
    {
        [SerializeField] private RingSpellController ringSpellController;
        [SerializeField] private SpellCaster spellCaster;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private HealthComponent health;
        [Tooltip("Необязательно. Если назначено — показывает активные эффекты и время до их окончания.")]
        [SerializeField] private StatusEffectController statusEffects;

        private SpellData _chargeSpell;
        private float _chargeStartTime;
        private bool _chargeReady;

        private void OnEnable()
        {
            spellCaster.OnChargeStarted += HandleChargeStarted;
            spellCaster.OnChargeReady += HandleChargeReady;
            spellCaster.OnChargeCancelled += HandleChargeEnded;
            spellCaster.OnSpellReleased += HandleChargeEnded;
        }

        private void OnDisable()
        {
            spellCaster.OnChargeStarted -= HandleChargeStarted;
            spellCaster.OnChargeReady -= HandleChargeReady;
            spellCaster.OnChargeCancelled -= HandleChargeEnded;
            spellCaster.OnSpellReleased -= HandleChargeEnded;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                mana.Restore(9999f);
            }
        }

        private void HandleChargeStarted(SpellData spell)
        {
            _chargeSpell = spell;
            _chargeStartTime = Time.time;
            _chargeReady = false;
        }

        private void HandleChargeReady(SpellData spell) => _chargeReady = true;

        private void HandleChargeEnded(SpellData spell) => _chargeSpell = null;

        private void OnGUI()
        {
            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            label.normal.textColor = Color.white;

            int statusCount = statusEffects != null ? statusEffects.ActiveEffects.Count : 0;

            const int width = 360;
            int height = 150 + statusCount * 22;
            float x = Screen.width * 0.5f - width * 0.5f;
            float y = Screen.height - height - 10;

            GUI.Box(new Rect(x, y, width, height), "");
            GUILayout.BeginArea(new Rect(x + 10, y + 8, width - 20, height - 16));

            SpellData spell = ringSpellController.ActiveSpell;
            GUILayout.Label($"Ring: {ringSpellController.ActiveRing?.Data.itemName ?? "-"}", label);
            GUILayout.Label($"Spell: {spell?.name ?? "-"}  ({spell?.resource} {spell?.cost})", label);
            GUILayout.Label($"Cooldown: {spellCaster.GetCooldownRemaining(spell):F2}s", label);
            GUILayout.Label($"Mana: {mana.CurrentMana:F0}/{mana.MaxMana:F0}   HP: {health.CurrentHealth}   (M — мана)", label);

            if (_chargeSpell == null)
            {
                GUILayout.Label("Charge: -", label);
            }
            else if (_chargeReady)
            {
                GUIStyle ready = new GUIStyle(label);
                ready.normal.textColor = Color.green;
                GUILayout.Label("Charge: READY — отпустите для каста", ready);
            }
            else
            {
                GUILayout.Label($"Charge: {Time.time - _chargeStartTime:F2} / {spellCaster.GetCastTime(_chargeSpell):F2}s", label);
            }

            if (statusEffects != null)
            {
                foreach (StatusEffectInstance effect in statusEffects.ActiveEffects)
                {
                    GUILayout.Label($"Status: {effect.Data.name} {effect.RemainingTime:F1}s", label);
                }
            }

            GUILayout.EndArea();
        }
    }
}
