using UnityEngine;
using Game.World;

namespace Game.Altars
{
    /// <summary>
    /// Алтарь в сцене. Сам ничего не делает: передаёт свои данные контроллеру игрока.
    /// Контроллер берётся у interactor (корень игрока), ссылкой из сцены на него не сослаться.
    /// </summary>
    public class Altar : MonoBehaviour, IInteractable
    {
        [SerializeField] private AltarData data;
        [SerializeField] private string prompt = "Отдохнуть";

        private AltarSessionController _controller;

        public bool CanInteract(GameObject interactor)
        {
            if (data == null) return false;

            AltarSessionController controller = GetController(interactor);
            return controller != null && !controller.IsActive;
        }

        public void Interact(GameObject interactor)
        {
            AltarSessionController controller = GetController(interactor);
            if (controller != null) controller.Begin(data);
        }

        public string GetInteractionPrompt() => prompt;

        // Кэш: CanInteract вызывается каждый кадр, пока игрок смотрит на алтарь.
        private AltarSessionController GetController(GameObject interactor)
        {
            if (_controller == null) _controller = interactor.GetComponent<AltarSessionController>();
            return _controller;
        }
    }
}