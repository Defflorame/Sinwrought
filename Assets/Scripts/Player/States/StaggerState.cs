using System;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Оглушение игрока: пока длится, нельзя начать ни одно действие и ходить (камера работает).
    /// Заходят сюда только через PlayerCombatController.TryStagger — он уже оборвал текущее действие.
    /// Время идёт через Tick, без корутин.
    /// </summary>
    public class StaggerState : IPlayerActionState
    {
        private readonly PlayerMovement _movement;
        private readonly Action _onFinished;

        private float _duration;
        private float _elapsed;

        public StaggerState(PlayerMovement movement, Action onFinished)
        {
            _movement = movement;
            _onFinished = onFinished;
        }

        public void SetDuration(float duration)
        {
            _duration = duration;
        }

        public void Enter()
        {
            _elapsed = 0f;
            _movement.SetMovementLocked(true);
        }

        public void Tick()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                _onFinished?.Invoke();
            }
        }

        public void Exit()
        {
            _movement.SetMovementLocked(false);
        }

        public bool CanInterrupt() => false;
    }
}