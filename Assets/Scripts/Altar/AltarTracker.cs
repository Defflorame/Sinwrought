using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Save;

namespace Game.Altars
{
    /// <summary>
    /// Runtime state алтарей игрока: какие активированы, у какого отдыхали последним
    /// и для каких разблокирован fast travel. Ключ — ссылка на AltarData.
    /// Только хранит, решений не принимает. Сохраняется как секция "altars".
    /// </summary>
    public class AltarTracker : MonoBehaviour, ISaveable
    {
        [Tooltip("Нужен для восстановления алтарей по Id из файла сохранения.")]
        [SerializeField] private AltarCatalogData catalog;

        private readonly HashSet<AltarData> _activated = new HashSet<AltarData>();
        private readonly List<AltarData> _fastTravel = new List<AltarData>();

        public AltarData LastAltar { get; private set; }

        /// <summary>Разблокированные для fast travel, в порядке разблокировки.</summary>
        public IReadOnlyList<AltarData> FastTravelAltars => _fastTravel;

        public string SaveKey => "altars";
        public int RestoreOrder => 0;

        public bool IsActivated(AltarData altar) => altar != null && _activated.Contains(altar);

        public bool IsFastTravelUnlocked(AltarData altar) => altar != null && _fastTravel.Contains(altar);

        public void RecordRest(AltarData altar)
        {
            if (altar == null) return;

            _activated.Add(altar);
            LastAltar = altar;
        }

        public void UnlockFastTravel(AltarData altar)
        {
            if (altar == null || _fastTravel.Contains(altar)) return;

            _fastTravel.Add(altar);
        }

        [Serializable]
        private class SaveData
        {
            public string lastAltarId;
            public List<string> activated = new List<string>();
            public List<string> fastTravel = new List<string>();
        }

        public string Capture()
        {
            var data = new SaveData { lastAltarId = LastAltar != null ? LastAltar.Id : "" };

            foreach (AltarData altar in _activated) data.activated.Add(altar.Id);
            foreach (AltarData altar in _fastTravel) data.fastTravel.Add(altar.Id);

            return JsonUtility.ToJson(data);
        }

        public void Restore(string json)
        {
            if (catalog == null)
            {
                Debug.LogWarning("AltarTracker: не назначен каталог алтарей, восстановление невозможно.", this);
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            _activated.Clear();
            _fastTravel.Clear();
            LastAltar = null;

            foreach (string id in data.activated)
            {
                if (TryResolve(id, out AltarData altar)) _activated.Add(altar);
            }

            foreach (string id in data.fastTravel)
            {
                if (TryResolve(id, out AltarData altar) && !_fastTravel.Contains(altar)) _fastTravel.Add(altar);
            }

            TryResolve(data.lastAltarId, out AltarData last);
            LastAltar = last;
        }

        private bool TryResolve(string id, out AltarData altar)
        {
            if (catalog.TryGet(id, out altar)) return true;

            if (!string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"AltarTracker: алтарь с Id '{id}' не найден в каталоге (пересоберите каталог).", this);
            }

            return false;
        }
    }
}