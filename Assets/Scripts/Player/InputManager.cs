using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Отвечает только за переключение Action Maps (Player / UI).
    /// Не содержит игровой логики.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        public InputActionAsset InputActions => inputActions;

        private InputActionMap _playerMap;
        private InputActionMap _uiMap;

        private void Awake()
        {
            _playerMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _uiMap = inputActions.FindActionMap("UI", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            SwitchToPlayer();
        }

        private void OnDisable()
        {
            DisableAllInput();
        }

        public void SwitchToPlayer()
        {
            _uiMap.Disable();
            _playerMap.Enable();
            SetCursorVisible(false);
        }

        public void SwitchToUI()
        {
            _playerMap.Disable();
            _uiMap.Enable();
            SetCursorVisible(true);
        }

        private static void SetCursorVisible(bool visible)
        {
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }

        public void DisableAllInput()
        {
            _playerMap.Disable();
            _uiMap.Disable();
        }
    }
}
