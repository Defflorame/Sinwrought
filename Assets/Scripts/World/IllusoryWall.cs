using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Иллюзорная стена как цель для SecretDetector: умеет временно «раскрываться». Открывание проходом — это WorldGate
    /// на том же объекте. Стена не знает, кто её раскрыл. Реестр нужен, чтобы детектор не искал по сцене.
    /// </summary>
    public class IllusoryWall : MonoBehaviour
    {
        private static readonly List<IllusoryWall> ActiveWalls = new List<IllusoryWall>();

        public static IReadOnlyList<IllusoryWall> All => ActiveWalls;

        public bool IsRevealed { get; private set; }

        public event Action OnRevealed;
        public event Action OnHidden;

        private Coroutine _hideRoutine;

        private void OnEnable()
        {
            ActiveWalls.Add(this);
        }

        private void OnDisable()
        {
            ActiveWalls.Remove(this);
            _hideRoutine = null;
            if (IsRevealed) Hide();
        }

        /// <summary>Раскрывает стену на duration секунд. Повторный вызов продлевает.</summary>
        public void Reveal(float duration)
        {
            if (_hideRoutine != null) StopCoroutine(_hideRoutine);

            if (!IsRevealed)
            {
                IsRevealed = true;
                OnRevealed?.Invoke();
            }

            _hideRoutine = StartCoroutine(HideAfter(duration));
        }

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            _hideRoutine = null;
            Hide();
        }

        private void Hide()
        {
            IsRevealed = false;
            OnHidden?.Invoke();
        }
    }
}