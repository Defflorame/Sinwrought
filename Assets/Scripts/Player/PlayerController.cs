using UnityEngine;
using Game.Combat;

namespace Game.Player
{
    /// <summary>
    /// Координатор игрока. НЕ содержит: формулы движения, логику камеры,
    /// расчёт урона, инвентарь, экипировку, магию.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerLook playerLook;
        [SerializeField] private PlayerCombatController combatController;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private PlayerInteractor interactor;
        private bool _isEnabled;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            inputReader.OnMove += HandleMove;
            inputReader.OnLook += HandleLook;
            inputReader.OnAttackPressed += HandleAttackPressed;
            inputReader.OnAttackReleased += HandleAttackReleased;
            inputReader.OnBlockPressed += HandleBlockPressed;
            inputReader.OnBlockReleased += HandleBlockReleased;
            inputReader.OnParryPressed += HandleParryPressed;
            inputReader.OnJumpPressed += HandleJumpPressed;
            inputReader.OnSwitchWeaponPressed += HandleSwitchWeaponPressed;
            inputReader.OnCastPressed += HandleCastPressed;
            inputReader.OnCastReleased += HandleCastReleased;
            inputReader.OnSwitchRing += HandleSwitchRing;
            inputReader.OnInteractPressed += HandleInteractPressed;
            healthComponent.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            inputReader.OnMove -= HandleMove;
            inputReader.OnLook -= HandleLook;
            inputReader.OnAttackPressed -= HandleAttackPressed;
            inputReader.OnAttackReleased -= HandleAttackReleased;
            inputReader.OnBlockPressed -= HandleBlockPressed;
            inputReader.OnBlockReleased -= HandleBlockReleased;
            inputReader.OnParryPressed -= HandleParryPressed;
            inputReader.OnJumpPressed -= HandleJumpPressed;
            inputReader.OnSwitchWeaponPressed -= HandleSwitchWeaponPressed;
            inputReader.OnCastPressed -= HandleCastPressed;
            inputReader.OnCastReleased -= HandleCastReleased;
            inputReader.OnSwitchRing -= HandleSwitchRing;
            inputReader.OnInteractPressed -= HandleInteractPressed;
            healthComponent.OnDeath -= HandleDeath;
        }

        public void Initialize()
        {
            EnablePlayer();
        }

        public void EnablePlayer()
        {
            _isEnabled = true;
            playerMovement.SetMovementLocked(false);
            playerLook.SetLookEnabled(true);
            interactor.SetInteractionEnabled(true);
        }

        public void DisablePlayer()
        {
            _isEnabled = false;
            playerMovement.SetMovementLocked(true);
            playerLook.SetLookEnabled(false);
            playerMovement.StopMovement();
            interactor.SetInteractionEnabled(false);
        }

        public void HandleDeath()
        {
            combatController.CancelAllActions();
            DisablePlayer();
        }

        public void HandleRespawn()
        {
            EnablePlayer();
        }

        private void HandleMove(Vector2 input)
        {
            if (!_isEnabled) return;
            playerMovement.Move(input);
        }

        private void HandleLook(Vector2 input)
        {
            if (!_isEnabled) return;
            playerLook.Look(input);
        }

        private void HandleAttackPressed()
        {
            if (!_isEnabled) return;
            combatController.OnAttackPressed();
        }

        private void HandleAttackReleased()
        {
            if (!_isEnabled) return;
            combatController.OnAttackReleased();
        }

        private void HandleBlockPressed()
        {
            if (!_isEnabled) return;
            combatController.StartBlockOrDodge();
        }

        private void HandleBlockReleased()
        {
            if (!_isEnabled) return;
            combatController.StopBlock();
        }

        private void HandleParryPressed()
        {
            if (!_isEnabled) return;
            combatController.TryParry();
        }

        private void HandleJumpPressed()
        {
            if (!_isEnabled) return;
            playerMovement.Jump();
        }

        private void HandleSwitchWeaponPressed()
        {
            if (!_isEnabled) return;
            combatController.TrySwitchWeapon();
        }

        private void HandleCastPressed()
        {
            if (!_isEnabled) return;
            combatController.TryCastSpell();
        }

        private void HandleCastReleased()
        {
            if (!_isEnabled) return;
            combatController.OnCastReleased();
        }

        private void HandleSwitchRing(int direction)
        {
            if (!_isEnabled) return;
            combatController.SwitchRing(direction);
        }
        private void HandleInteractPressed()
        {
            if (!_isEnabled) return;
            if (!combatController.CanPerformAction()) return;
            interactor.TryInteract();
        }
    }
}