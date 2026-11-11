using System;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Состояния поведения врага (Idle, Patrol, Chase...). Это НЕ GameStateMachine и НЕ
    /// PlayerActionStateMachine. Переходы решают сами состояния и EnemyBrain.
    /// </summary>
    public class EnemyStateMachine : MonoBehaviour
    {
        public IEnemyState CurrentState { get; private set; }

        /// <summary>Вызывается при смене состояния ДО Enter() нового.</summary>
        public event Action<IEnemyState> OnStateChanged;

        private void Update()
        {
            CurrentState?.Tick();
        }

        /// <summary>Безусловно переключает состояние. Повторный вход в то же состояние (Exit → Enter) разрешён.</summary>
        public void ChangeState(IEnemyState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
            CurrentState?.Enter();
        }
    }
}
