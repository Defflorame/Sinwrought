using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Отображает состояние WorldGate в Animator: ведёт float Progress (0 — закрыто, 1 — открыто) к цели,
    /// клип проигрывается через Motion Time. Смена цели посреди движения разворачивает створку с текущего
    /// положения — без рывков и без ожидания конца клипа. Игровых решений драйвер не принимает.
    /// Один драйвер на каждый Animator: дверь и рукоять рычага слушают одни и те же ворота.
    /// Update работает только пока створка движется.
    /// </summary>
    public class GateAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private WorldGate gate;
        [SerializeField] private Animator animator;

        [Tooltip("Секунд на полное открытие/закрытие. 0 или меньше — длина первого клипа контроллера.")]
        [SerializeField] private float duration = 0f;

        private float _progress;
        private float _target;
        private float _resolvedDuration;
        private bool _moving;

        private void Awake()
        {
            _resolvedDuration = duration > 0f ? duration : GetClipLength();
        }


        private void OnEnable()
        {
            gate.OnOpened += HandleOpened;
            gate.OnClosed += HandleClosed;
        }

        private void OnDisable()
        {
            gate.OnOpened -= HandleOpened;
            gate.OnClosed -= HandleClosed;
        }

        private void Start()
        {
            // Начальное положение — мгновенно, без анимации.
            _target = gate.IsOpen ? 1f : 0f;
            _progress = _target;
            animator.SetFloat(GateAnimatorIds.Progress, _progress);
        }

        private void HandleOpened() => SetTarget(1f);
        private void HandleClosed() => SetTarget(0f);

        private void SetTarget(float target)
        {
            _target = target;
            _moving = true;
        }

        private void Update()
        {
            if (!_moving) return;

            float step = _resolvedDuration > 0f ? Time.deltaTime / _resolvedDuration : 1f;
            _progress = Mathf.MoveTowards(_progress, _target, step);
            animator.SetFloat(GateAnimatorIds.Progress, _progress);

            if (Mathf.Approximately(_progress, _target)) _moving = false;
        }

        private float GetClipLength()
        {
            AnimationClip[] clips = animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.animationClips
                : null;

            return clips != null && clips.Length > 0 ? clips[0].length : 1f;
        }
    }
}