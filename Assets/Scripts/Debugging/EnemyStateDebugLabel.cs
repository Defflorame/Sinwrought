using UnityEngine;
using Game.Combat;
using Game.Enemies;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Над врагом показывает состояние, HP и устойчивость к оглушению
    /// (текущая / нужная, либо «immune»). Виден и в окне Game (OnGUI), и в окне Scene (Handles).
    /// Только читает EnemyStateMachine, HealthComponent и EnemyBrain.Poise.
    /// </summary>
    public class EnemyStateDebugLabel : MonoBehaviour
    {
        [SerializeField] private float heightOffset = 2.6f;
        [SerializeField] private int fontSize = 22;
        [SerializeField] private bool showInGameView = true;
        [SerializeField] private bool showInSceneView = true;

        private EnemyStateMachine _machine;
        private EnemyBrain _brain;
        private EnemyController _controller;
        private HealthComponent _health;
        private Camera _camera;

        private void Awake()
        {
            _machine = GetComponent<EnemyStateMachine>();
            _brain = GetComponent<EnemyBrain>();
            _health = GetComponent<HealthComponent>();
            _controller = GetComponent<EnemyController>();
        }

        private bool TryBuildText(out string text)
        {
            text = null;
            if (_machine == null || _machine.CurrentState == null) return false;

            string state = _machine.CurrentState.GetType().Name.Replace("Enemy", "").Replace("State", "");
            string hp = _health != null ? $"HP {_health.CurrentHealth}/{_health.MaxHealth}" : "";

            string poise = "Stagger: -";
            PoiseTracker p = _brain != null ? _brain.Poise : null;
            if (p != null)
            {
                bool immune = _controller != null && _controller.Data != null && _controller.Data.immuneToStagger;
                poise = immune
                    ? "Stagger: IMMUNE (не оглушить)"
                    : $"Stagger: {Mathf.Max(0f, p.Current):F0} / {p.Max:F0}";
            }

            text = $"{state}   {hp}\n{poise}";
            return true;
        }

        private void OnGUI()
        {
            if (!showInGameView || !TryBuildText(out string text)) return;

            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            Vector3 screen = _camera.WorldToScreenPoint(transform.position + Vector3.up * heightOffset);
            if (screen.z <= 0f) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            Rect rect = new Rect(screen.x - 200f, Screen.height - screen.y - fontSize * 1.5f, 400f, fontSize * 3f);

            style.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, style);
            style.normal.textColor = Color.white;
            GUI.Label(rect, text, style);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showInSceneView || !Application.isPlaying || !TryBuildText(out string text)) return;

            GUIStyle style = new GUIStyle
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.white;

            UnityEditor.Handles.Label(transform.position + Vector3.up * heightOffset, text, style);
        }
#endif
    }
}
