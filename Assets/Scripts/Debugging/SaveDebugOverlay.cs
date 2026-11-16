// Game.Debugging/SaveDebugOverlay.cs
using UnityEngine;
using Game.Altars;
using Game.Save;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Удалить в Phase 14 вместе с остальными overlay.
    /// Проверка файла сохранения: Save / Load (применить к живому игроку без смены сцены) / Delete.
    /// </summary>
    public class SaveDebugOverlay : MonoBehaviour
    {
        [SerializeField] private SaveController saveController;
        [SerializeField] private AltarTracker tracker;

        private string _status = "";

        private void OnGUI()
        {
            if (saveController == null || tracker == null) return;

            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            label.normal.textColor = Color.white;
            GUIStyle header = new GUIStyle(label) { fontStyle = FontStyle.Bold };

            const int width = 320;
            const int left = 380;
            const int top = 440;
            const int height = 230;

            GUI.Box(new Rect(left, top, width, height), "");
            GUILayout.BeginArea(new Rect(left + 10, top + 8, width - 20, height - 16));

            GUILayout.Label("Save (debug)", header);
            GUILayout.Label($"Файл: {(SaveFileService.Exists() ? "есть" : "нет")}", label);

            if (SaveFileService.TryRead(out SaveGameData data))
            {
                GUILayout.Label($"v{data.version}, алтарь {Short(data.altarId)}, секций {data.sections.Count}", label);
            }

            GUILayout.Label($"Last altar: {(tracker.LastAltar != null ? tracker.LastAltar.AltarName : "-")}", label);
            GUILayout.Label($"Fast travel: {tracker.FastTravelAltars.Count}", label);

            GUI.enabled = tracker.LastAltar != null;
            if (GUILayout.Button("Save"))
            {
                _status = saveController.SaveNow(tracker.LastAltar.Id) ? "сохранено" : "ошибка записи";
            }
            GUI.enabled = true;

            if (GUILayout.Button("Load (apply)"))
            {
                if (SaveFileService.TryRead(out SaveGameData loaded))
                {
                    saveController.Apply(loaded);
                    _status = "применено";
                }
                else
                {
                    _status = "нет действительного сохранения";
                }
            }

            if (GUILayout.Button("Delete"))
            {
                SaveFileService.Delete();
                _status = "удалено";
            }

            GUILayout.Label(_status, label);
            GUILayout.EndArea();
        }

        private static string Short(string id) => string.IsNullOrEmpty(id) ? "-" : id.Substring(0, Mathf.Min(6, id.Length));
    }
}