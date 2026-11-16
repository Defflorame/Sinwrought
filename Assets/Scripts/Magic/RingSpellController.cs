using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Items;
using Game.Player.Equipment;
using Game.Save;

namespace Game.Magic
{
    /// <summary>
    /// Хранит, какое из надетых колец с заклинанием сейчас активно, и
    /// переключает его. Количество колец не захардкожено — берётся из PlayerEquipment.
    /// Не кастует: за это отвечают CastState и SpellCaster.
    /// </summary>
    public class RingSpellController : MonoBehaviour, ISaveable
    {
        [SerializeField] private PlayerEquipment playerEquipment;

        private readonly List<ItemInstance> _spellRings = new List<ItemInstance>();
        private int _activeIndex;

        public event Action OnActiveRingChanged;

        public ItemInstance ActiveRing => _spellRings.Count > 0 ? _spellRings[_activeIndex] : null;
        public SpellData ActiveSpell => (ActiveRing?.Data as RingData)?.Spell;

        public string SaveKey => "activeRing";
        public int RestoreOrder => 30;

        private void OnEnable()
        {
            playerEquipment.OnEquipmentChanged += Rebuild;
            Rebuild();
        }

        private void OnDisable()
        {
            playerEquipment.OnEquipmentChanged -= Rebuild;
        }

        /// <summary>direction: +1 следующее кольцо, -1 предыдущее.</summary>
        public void SwitchRing(int direction)
        {
            if (_spellRings.Count < 2) return;

            _activeIndex = (_activeIndex + direction + _spellRings.Count) % _spellRings.Count;
            Debug.Log($"RingSpellController: активное кольцо — {ActiveRing.Data.itemName}");
            OnActiveRingChanged?.Invoke();
        }

        private void Rebuild()
        {
            ItemInstance previous = ActiveRing;

            _spellRings.Clear();
            foreach (ItemInstance ring in playerEquipment.EquippedRings)
            {
                if (ring.Data is RingData ringData && ringData.Spell != null)
                {
                    _spellRings.Add(ring);
                }
            }

            int index = previous != null ? _spellRings.IndexOf(previous) : 0;
            _activeIndex = Mathf.Max(0, index);
            OnActiveRingChanged?.Invoke();
        }

        [Serializable]
        private class SaveData
        {
            public int activeIndex;
        }

        public string Capture() => JsonUtility.ToJson(new SaveData { activeIndex = _activeIndex });

        public void Restore(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null || _spellRings.Count == 0) return;

            _activeIndex = Mathf.Clamp(data.activeIndex, 0, _spellRings.Count - 1);
            OnActiveRingChanged?.Invoke();
        }
    }
}