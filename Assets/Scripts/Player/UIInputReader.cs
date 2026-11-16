using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Читает ввод карты UI и переводит его в команды (события).
    /// Единственный класс, знающий о действиях карты UI. Не решает, что происходит после команды.
    /// </summary>
    public class UIInputReader : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;

        public event Action OnCancelPressed;

        private InputAction _cancelAction;

        private void Awake()
        {
            InputActionMap uiMap = inputManager.InputActions.FindActionMap("UI", throwIfNotFound: true);
            _cancelAction = uiMap.FindAction("Cancel", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _cancelAction.started += HandleCancelPressed;
        }

        private void OnDisable()
        {
            _cancelAction.started -= HandleCancelPressed;
        }

        private void HandleCancelPressed(InputAction.CallbackContext ctx) => OnCancelPressed?.Invoke();
    }
}