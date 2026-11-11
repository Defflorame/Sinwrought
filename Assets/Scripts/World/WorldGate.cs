using System;
using System.Collections;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Проход: состояние «открыто/закрыто», требования, проходимость и автозакрытие. Картинкой не занимается:
    /// двигает дверь GateTransformAnimator, подписанный на OnOpened/OnClosed. Кто открывает — не знает
    /// (GateInteractable, SpellGateOpener).
    /// Закрывается (по таймеру и вручную) только если в проходе никого нет.
    ///
    /// TODO Phase 11 (ОБЯЗАТЕЛЬНО): состояние двери, открытой ключом (closeDelay = 0), должно сохраняться при смене
    /// локации и выходе из игры: PersistentId + WorldStateService и метод восстановления открытого состояния
    /// БЕЗ проверки требований и без расхода ключа (и событие с флагом «мгновенно» для аниматора). До этого
    /// момента перезагрузка сцены закрывает дверь, а ключ уже потрачен.
    /// </summary>
    public class WorldGate : MonoBehaviour
    {
        [Tooltip("Все требования должны быть выполнены. Пусто — открывается без условий.")]
        [SerializeField] private AccessRequirement[] requirements;

        [Tooltip("Твёрдые коллайдеры двери/барьера. Отключаются при открытии и включаются при закрытии. Обычно — дети двигающегося объекта.")]
        [SerializeField] private Collider[] solidColliders;

        [Tooltip("Включено — коллайдеры отключаются на время открытия (для SpellGate). Выключено — коллайдер всегда ощущается и едет вместе с дверью.")]
        [SerializeField] private bool disableCollidersWhenOpen = false;

        [Min(0f)]
        [Tooltip("Через сколько секунд после открытия ворота закрываются сами. 0 — остаются открытыми (или закрываются вручную, как у рычага).")]
        [SerializeField] private float closeDelay = 0f;

        [Header("Препятствия при закрытии")]
        [Tooltip("Слои, которых не должно быть в проходе при закрытии (Player, Enemy). Для любых ворот, которые могут закрываться, иначе закроются на игроке.")]
        [SerializeField] private LayerMask obstructionMask;
        [SerializeField] private float obstructionRetryInterval = 0.25f;
        [Tooltip("Насколько сузить проверяемый объём, чтобы стоящий рядом не мешал закрытию.")]
        [SerializeField] private float obstructionShrink = 0.05f;

        public bool IsOpen { get; private set; }

        public event Action OnOpened;
        public event Action OnClosed;

        private Bounds _passageBounds;
        private bool _hasBounds;
        private Coroutine _closeRoutine;

        private void Awake()
        {
            // Считаем, пока дверь закрыта: это и есть проём, который нельзя закрывать на ком-то.
            CachePassageBounds();

            if (closeDelay > 0f && obstructionMask.value == 0)
            {
                Debug.LogWarning($"WorldGate: у {name} закрытие по таймеру, но не задан obstructionMask — ворота закроются на игроке.", this);
            }
        }

        /// <summary>Проверяет требования, забирает расходуемое и открывает. false — ничего не изменено.</summary>
        public bool TryOpen(GameObject opener)
        {
            if (IsOpen) return false;

            if (!AccessRequirement.AreAllMet(requirements, opener))
            {
                Debug.Log($"WorldGate: {name} — требования не выполнены");
                return false;
            }

            if (requirements != null)
            {
                foreach (AccessRequirement requirement in requirements)
                {
                    requirement.Consume(opener);
                }
            }

            Open();
            return true;
        }

        /// <summary>Закрывает вручную (рычаг). false — уже закрыто или в проходе кто-то стоит.</summary>
        public bool TryClose()
        {
            if (!IsOpen) return false;

            if (IsObstructed())
            {
                Debug.Log($"WorldGate: {name} — проход занят, закрыть нельзя");
                return false;
            }

            StopCloseRoutine();
            Close();
            return true;
        }

        private void Open()
        {
            IsOpen = true;
            SetSolid(false);
            Debug.Log($"WorldGate: {name} открыт");
            OnOpened?.Invoke();

            if (closeDelay > 0f)
            {
                StopCloseRoutine();
                _closeRoutine = StartCoroutine(CloseAfterDelay());
            }
        }

        private void Close()
        {
            IsOpen = false;
            SetSolid(true);
            Debug.Log($"WorldGate: {name} закрыт");
            OnClosed?.Invoke();
        }

        private IEnumerator CloseAfterDelay()
        {
            yield return new WaitForSeconds(closeDelay);

            // Что-то стоит в проходе — не закрываемся, пробуем снова.
            while (IsObstructed())
            {
                yield return new WaitForSeconds(obstructionRetryInterval);
            }

            _closeRoutine = null;
            Close();
        }

        private void StopCloseRoutine()
        {
            if (_closeRoutine == null) return;

            StopCoroutine(_closeRoutine);
            _closeRoutine = null;
        }

        private void SetSolid(bool solid)
        {
            if (!disableCollidersWhenOpen || solidColliders == null) return;

            foreach (Collider collider in solidColliders)
            {
                if (collider != null) collider.enabled = solid;
            }
        }

        private void CachePassageBounds()
        {
            _hasBounds = false;
            if (solidColliders == null) return;

            foreach (Collider collider in solidColliders)
            {
                if (collider == null) continue;

                if (!_hasBounds)
                {
                    _passageBounds = collider.bounds;
                    _hasBounds = true;
                }
                else
                {
                    _passageBounds.Encapsulate(collider.bounds);
                }
            }
        }

        private bool IsObstructed()
        {
            if (!_hasBounds || obstructionMask.value == 0) return false;

            Vector3 halfExtents = Vector3.Max(_passageBounds.extents - Vector3.one * obstructionShrink, Vector3.one * 0.01f);
            return Physics.CheckBox(_passageBounds.center, halfExtents, Quaternion.identity, obstructionMask, QueryTriggerInteraction.Ignore);
        }
    }
}