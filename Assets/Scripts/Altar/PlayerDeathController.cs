// Game.Altars/PlayerDeathController.cs
using UnityEngine;
using Game.Combat;

namespace Game.Altars
{
    /// <summary>
    /// Смерть игрока = загрузка последнего сохранения (после короткой паузы).
    /// Если сохранения нет, ничего не делает: остаётся отладочное воскрешение.
    /// Компонент выключен, пока игрок жив (таймер идёт в Update только после смерти).
    /// </summary>
    public class PlayerDeathController : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private GameLoadController loadController;
        [Min(0f)]
        [Tooltip("Сколько секунд между смертью и загрузкой.")]
        [SerializeField] private float loadDelay = 2f;

        private float _timer;

        private void Awake()
        {
            enabled = false;
            health.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (!loadController.HasSave)
            {
                Debug.Log("PlayerDeathController: сохранения нет, загрузка не выполняется.", this);
                return;
            }

            _timer = loadDelay;
            enabled = true;
        }

        private void Update()
        {
            // Игрока воскресили до загрузки (отладочный Respawn): загрузка не нужна.
            if (!health.IsDead())
            {
                enabled = false;
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            enabled = false;
            if (!loadController.LoadLast())
            {
                Debug.LogWarning("PlayerDeathController: загрузка не началась.", this);
            }
        }
    }
}