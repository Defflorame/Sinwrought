using UnityEngine;
using Game.Altars;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Удалить, когда появится полноценный UI алтаря (Phase 14).
    /// Показывает активную сессию алтаря, список fast travel, кнопки «Сохранить» и выхода.
    /// Только читает состояние и вызывает команды контроллера. Очки тратятся в ProgressionDebugOverlay.
    /// </summary>
    public class AltarDebugOverlay : MonoBehaviour
    {
        [SerializeField] private AltarSessionController session;
        [SerializeField] private AltarTracker tracker;

        private string _saveStatus = "";

        private void OnGUI()
        {
            if (session == null || tracker == null || !session.IsActive)
            {
                _saveStatus = "";
                return;
            }

            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            label.normal.textColor = Color.white;
            GUIStyle header = new GUIStyle(label) { fontStyle = FontStyle.Bold };

            var targets = tracker.FastTravelAltars;

            const int width = 360;
            const int left = 10;
            const int top = 440; // под EquipmentDebugTool
            int height = 190 + targets.Count * 30;

            GUI.Box(new Rect(left, top, width, height), "");
            GUILayout.BeginArea(new Rect(left + 10, top + 8, width - 20, height - 16));

            AltarData current = session.CurrentAltar;
            GUILayout.Label($"Алтарь: {current.AltarName}", header);
            GUILayout.Label($"Fast travel: {(tracker.IsFastTravelUnlocked(current) ? "разблокирован" : "закрыт")}", label);

            GUILayout.Label("Перейти к алтарю:", header);
            foreach (AltarData altar in targets)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(altar == current ? $"{altar.AltarName} (здесь)" : altar.AltarName, label, GUILayout.Width(230));
                GUI.enabled = session.CanFastTravelTo(altar);
                if (GUILayout.Button("Travel", GUILayout.Width(70)))
                {
                    session.TryFastTravel(altar);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Сохранить", GUILayout.Height(28)))
            {
                _saveStatus = session.TrySave() ? "Сохранено" : "Ошибка сохранения";
            }
            if (_saveStatus.Length > 0) GUILayout.Label(_saveStatus, label);

            if (GUILayout.Button("Покинуть (Esc)", GUILayout.Height(28)))
            {
                session.End();
            }

            GUILayout.EndArea();
        }
    }
}