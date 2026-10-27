using System;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// «Можно ли сейчас тратить очки». Очки статов и способностей тратятся только у кристалла:
    /// кристалл (Phase 10) вызывает Open() при взаимодействии и Close() при уходе.
    /// Пока кристаллов нет, переключает ProgressionDebugOverlay.
    /// </summary>
    public class PointSpendingGate : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        public event Action<bool> OnGateChanged;

        public void Open() => SetOpen(true);

        public void Close() => SetOpen(false);

        private void SetOpen(bool open)
        {
            if (IsOpen == open) return;

            IsOpen = open;
            OnGateChanged?.Invoke(open);
        }
    }
}
