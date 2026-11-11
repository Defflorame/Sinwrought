using UnityEngine;
using Game.Player;
using Game.World;

namespace Game.Debugging
{
    /// <summary>ВРЕМЕННЫЙ инструмент. Удалить в Phase 14 (HUD). Показывает подсказку взаимодействия по центру экрана.</summary>
    public class InteractionPromptDebugOverlay : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;

        private IInteractable _target;

        private void OnEnable()
        {
            interactor.OnTargetChanged += HandleTargetChanged;
        }

        private void OnDisable()
        {
            interactor.OnTargetChanged -= HandleTargetChanged;
        }

        private void HandleTargetChanged(IInteractable target)
        {
            _target = target;
        }

        private void OnGUI()
        {
            if (_target == null) return;

            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.6f, 300f, 40f), $"[E] {_target.GetInteractionPrompt()}", style);
        }
    }
}