// Game.Save/SaveController.cs
using System.Collections.Generic;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Собирает секции со всех ISaveable на игроке и его детях и пишет файл (SaveNow);
    /// раздаёт секции обратно (Apply). Типов секций не знает: новая система (карта, квесты)
    /// просто реализует ISaveable. Когда сохраняться, решает вызывающий (алтарь).
    /// </summary>
    public class SaveController : MonoBehaviour
    {
        /// <summary>altarId передаётся параметром, чтобы Game.Save не зависел от Game.Altars.</summary>
        public bool SaveNow(string altarId)
        {
            if (string.IsNullOrEmpty(altarId))
            {
                Debug.LogWarning("SaveController: не задан altarId, сохранение отменено.", this);
                return false;
            }

            var data = new SaveGameData { altarId = altarId };
            var usedKeys = new HashSet<string>();

            foreach (ISaveable saveable in GetComponentsInChildren<ISaveable>(true))
            {
                if (!usedKeys.Add(saveable.SaveKey))
                {
                    Debug.LogWarning($"SaveController: ключ секции '{saveable.SaveKey}' повторяется, вторая секция пропущена.", this);
                    continue;
                }

                data.sections.Add(new SaveSectionData { key = saveable.SaveKey, json = saveable.Capture() });
            }

            bool written = SaveFileService.TryWrite(data);
            if (written) Debug.Log($"SaveController: сохранено ({data.sections.Count} секций), алтарь {altarId}");
            return written;
        }

        /// <summary>Заменяет состояние компонентов данными файла. Секции без данных в файле пропускаются.</summary>
        public void Apply(SaveGameData data)
        {
            if (data == null) return;

            var saveables = new List<ISaveable>(GetComponentsInChildren<ISaveable>(true));
            saveables.Sort((a, b) => a.RestoreOrder.CompareTo(b.RestoreOrder));

            foreach (ISaveable saveable in saveables)
            {
                SaveSectionData section = data.sections.Find(s => s.key == saveable.SaveKey);
                if (section == null)
                {
                    Debug.LogWarning($"SaveController: в файле нет секции '{saveable.SaveKey}'.", this);
                    continue;
                }

                saveable.Restore(section.json);
            }
        }
    }
}