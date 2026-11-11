using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Делает WorldGate доступным по кнопке Interact. toggle выключен — только открывает (дверь с ключом, иллюзорная стена);
    /// включён — рычаг: повторное нажатие закрывает. Подсказка зависит от текущего состояния ворот.
    /// </summary>
    public class GateInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private WorldGate gate;
        [SerializeField] private string openPrompt = "Открыть";
        [SerializeField] private string closePrompt = "Закрыть";
        [Tooltip("Рычаг: когда ворота открыты, нажатие закрывает их.")]
        [SerializeField] private bool toggle = false;

        public bool CanInteract(GameObject interactor) => gate != null && (toggle || !gate.IsOpen);

        public void Interact(GameObject interactor)
        {
            if (gate == null) return;

            if (gate.IsOpen) gate.TryClose();
            else gate.TryOpen(interactor);
        }

        public string GetInteractionPrompt() => gate != null && gate.IsOpen ? closePrompt : openPrompt;
    }
}