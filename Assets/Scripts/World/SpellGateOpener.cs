using UnityEngine;
using Game.Combat;
using Game.Magic;

namespace Game.World
{
    /// <summary>
    /// Открывает WorldGate, когда в SpellReceiver попадает нужное заклинание. casterLayers не даёт
    /// врагам открывать ворота своими заклинаниями.
    /// </summary>
    public class SpellGateOpener : MonoBehaviour
    {
        [SerializeField] private SpellReceiver receiver;
        [SerializeField] private WorldGate gate;
        [SerializeField] private SpellData requiredSpell;
        [Tooltip("Слои кастеров, которым разрешено открывать (обычно Player).")]
        [SerializeField] private LayerMask casterLayers;

        private void OnEnable()
        {
            receiver.OnSpellReceived += HandleSpellReceived;
        }

        private void OnDisable()
        {
            receiver.OnSpellReceived -= HandleSpellReceived;
        }

        private void HandleSpellReceived(SpellData spell, GameObject caster)
        {
            if (spell != requiredSpell || caster == null) return;
            if (!LayerMaskExtensions.Contains(casterLayers, caster.layer)) return;

            gate.TryOpen(caster);
        }
    }
}