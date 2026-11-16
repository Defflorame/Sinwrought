// Game.Save/SaveGameData.cs
using System;
using System.Collections.Generic;

namespace Game.Save
{
    /// <summary>Содержимое файла сохранения. Только данные. Словарей нет: JsonUtility их не умеет.</summary>
    [Serializable]
    public class SaveGameData
    {
        /// <summary>Менять при несовместимом изменении формата: старые файлы тогда считаются недействительными.</summary>
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        /// <summary>Алтарь, у которого сохранились: сюда ставим игрока при Load (11.2).</summary>
        public string altarId;

        public List<SaveSectionData> sections = new List<SaveSectionData>();
    }
}