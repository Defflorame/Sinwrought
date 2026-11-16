// Game.Save/SaveFileService.cs
using System;
using System.IO;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Работа с файлом сохранения (один слот). Запись через временный файл:
    /// если игра вылетит посреди записи, старое сохранение останется целым.
    /// </summary>
    public static class SaveFileService
    {
        private const string FileName = "save.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool Exists() => File.Exists(FilePath);

        public static bool TryWrite(SaveGameData data)
        {
            string path = FilePath;
            string tempPath = path + ".tmp";

            try
            {
                File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));

                if (File.Exists(path)) File.Replace(tempPath, path, null);
                else File.Move(tempPath, path);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveFileService: не удалось записать сохранение: {e.Message}");
                return false;
            }
        }

        /// <summary>false — файла нет, он повреждён или его версия не совпадает с текущей.</summary>
        public static bool TryRead(out SaveGameData data)
        {
            data = null;
            if (!Exists()) return false;

            try
            {
                data = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveFileService: не удалось прочитать сохранение: {e.Message}");
                return false;
            }

            if (data == null || data.sections == null)
            {
                Debug.LogWarning("SaveFileService: файл сохранения пуст или повреждён.");
                data = null;
                return false;
            }

            if (data.version != SaveGameData.CurrentVersion)
            {
                Debug.LogWarning($"SaveFileService: версия сохранения {data.version}, нужна {SaveGameData.CurrentVersion}. Файл игнорируется.");
                data = null;
                return false;
            }

            return true;
        }

        public static void Delete()
        {
            try
            {
                if (Exists()) File.Delete(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveFileService: не удалось удалить сохранение: {e.Message}");
            }
        }
    }
}