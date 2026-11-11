using System;
using UnityEngine;
using Game.World;

namespace Game.Player
{
    /// <summary>
    /// Ищет объект взаимодействия перед камерой и вызывает его по команде. Рейкаст каждый кадр нужен,
    /// чтобы подсказка появлялась при подходе (события здесь нет). Сообщает о смене цели — для будущего HUD.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Tooltip("Камера игрока.")]
        [SerializeField] private Transform origin;
        [SerializeField] private float interactDistance = 2.5f;
        [Tooltip("Слои, которые луч считает преградой/целью (стены, двери, враги).")]
        [SerializeField] private LayerMask interactMask = ~0;

        public IInteractable CurrentTarget { get; private set; }

        public event Action<IInteractable> OnTargetChanged;

        private bool _isEnabled = true;

        private void Update()
        {
            if (!_isEnabled) return;

            IInteractable found = null;

            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
                if (found != null && !found.CanInteract(gameObject)) found = null;
            }

            SetTarget(found);
        }

        public void TryInteract()
        {
            if (!_isEnabled || CurrentTarget == null) return;

            CurrentTarget.Interact(gameObject);
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            if (!isEnabled) SetTarget(null);
        }

        private void SetTarget(IInteractable target)
        {
            if (target == CurrentTarget) return;

            CurrentTarget = target;
            OnTargetChanged?.Invoke(target);
        }
    }
}