using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Единственный класс, знающий о конкретных Input Actions.
    /// Переводит физический ввод в игровые команды (события).
    /// Не решает, что происходит после команды.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;

        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnAttackPressed;
        public event Action OnAttackReleased;
        public event Action OnBlockPressed;
        public event Action OnBlockReleased;
        public event Action OnParryPressed;
        public event Action OnJumpPressed;
        public event Action OnSwitchWeaponPressed;
        public event Action OnCastPressed;
        public event Action OnCastReleased;
        public event Action<int> OnSwitchRing;
        public event Action OnInteractPressed;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _blockAction;
        private InputAction _parryAction;
        private InputAction _jumpAction;
        private InputAction _switchWeaponAction;
        private InputAction _castAction;
        private InputAction _switchRingAction;
        private InputAction _interactAction;


        private void Awake()
        {
            InputActionMap playerMap = inputManager.InputActions.FindActionMap("Player", throwIfNotFound: true);

            _moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
            _lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
            _attackAction = playerMap.FindAction("Attack", throwIfNotFound: true);
            _blockAction = playerMap.FindAction("Block", throwIfNotFound: true);
            _parryAction = playerMap.FindAction("Parry", throwIfNotFound: true);
            _jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
            _switchWeaponAction = playerMap.FindAction("SwitchWeapon", throwIfNotFound: true);
            _castAction = playerMap.FindAction("Cast", throwIfNotFound: true);
            _switchRingAction = playerMap.FindAction("SwitchRing", throwIfNotFound: true);
            _interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);


            // Cursor.visible = false;
            // Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnEnable()
        {
            _attackAction.started += HandleAttackPressed;
            _attackAction.canceled += HandleAttackReleased;
            _blockAction.started += HandleBlockPressed;
            _blockAction.canceled += HandleBlockReleased;
            _parryAction.started += HandleParryPressed;
            _jumpAction.started += HandleJumpPressed;
            _switchWeaponAction.started += HandleSwitchWeaponPressed;
            _castAction.started += HandleCastPressed;
            _castAction.canceled += HandleCastReleased;
            _switchRingAction.performed += HandleSwitchRing;
            _interactAction.started += HandleInteractPressed;
        }

        private void OnDisable()
        {
            _attackAction.started -= HandleAttackPressed;
            _attackAction.canceled -= HandleAttackReleased;
            _blockAction.started -= HandleBlockPressed;
            _blockAction.canceled -= HandleBlockReleased;
            _parryAction.started -= HandleParryPressed;
            _jumpAction.started -= HandleJumpPressed;
            _switchWeaponAction.started -= HandleSwitchWeaponPressed;
            _castAction.started -= HandleCastPressed;
            _castAction.canceled -= HandleCastReleased;
            _switchRingAction.performed -= HandleSwitchRing;
            _interactAction.started -= HandleInteractPressed;
        }

        private void Update()
        {
            OnMove?.Invoke(_moveAction.ReadValue<Vector2>());
            OnLook?.Invoke(_lookAction.ReadValue<Vector2>());
        }

        private void HandleAttackPressed(InputAction.CallbackContext ctx) => OnAttackPressed?.Invoke();
        private void HandleAttackReleased(InputAction.CallbackContext ctx) => OnAttackReleased?.Invoke();
        private void HandleBlockPressed(InputAction.CallbackContext ctx) => OnBlockPressed?.Invoke();
        private void HandleBlockReleased(InputAction.CallbackContext ctx) => OnBlockReleased?.Invoke();
        private void HandleParryPressed(InputAction.CallbackContext ctx) => OnParryPressed?.Invoke();
        private void HandleJumpPressed(InputAction.CallbackContext ctx) => OnJumpPressed?.Invoke();
        private void HandleSwitchWeaponPressed(InputAction.CallbackContext ctx) => OnSwitchWeaponPressed?.Invoke();
        private void HandleCastPressed(InputAction.CallbackContext ctx) => OnCastPressed?.Invoke();
        private void HandleCastReleased(InputAction.CallbackContext ctx) => OnCastReleased?.Invoke();

        private void HandleSwitchRing(InputAction.CallbackContext ctx)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            if (Mathf.Approximately(scroll, 0f)) return;

            OnSwitchRing?.Invoke(scroll > 0f ? 1 : -1);
        }
        private void HandleInteractPressed(InputAction.CallbackContext ctx) => OnInteractPressed?.Invoke();
    }
}