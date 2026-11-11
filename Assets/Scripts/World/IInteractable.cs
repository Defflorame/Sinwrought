using UnityEngine;

namespace Game.World
{
    /// <summary>Объект, с которым можно взаимодействовать кнопкой Interact. interactor — корень игрока.</summary>
    public interface IInteractable
    {
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
        string GetInteractionPrompt();
    }
}