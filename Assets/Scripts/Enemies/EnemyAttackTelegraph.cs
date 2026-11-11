using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Временный визуальный телеграф замаха для моделей-кубов: подсвечивает врага на время замаха —
    /// жёлтым, если атаку можно парировать, красным, если нет. Только отображает: слушает события
    /// EnemyCombatController. Когда появятся скелет и анимации — заменить аниматором/VFX/звуком.
    /// </summary>
    public class EnemyAttackTelegraph : MonoBehaviour
    {
        [Tooltip("Пусто — берутся все Renderer врага.")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color parryableColor = new Color(1f, 0.9f, 0.1f);
        [SerializeField] private Color unparryableColor = new Color(1f, 0.1f, 0.1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private EnemyCombatController _combat;
        private MaterialPropertyBlock _block;

        private void Awake()
        {
            _combat = GetComponent<EnemyCombatController>();
            _block = new MaterialPropertyBlock();

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
        }

        private void OnEnable()
        {
            _combat.OnWindupStarted += HandleWindupStarted;
            _combat.OnStrike += ClearHighlight;
            _combat.OnAttackEnded += ClearHighlight;
        }

        private void OnDisable()
        {
            _combat.OnWindupStarted -= HandleWindupStarted;
            _combat.OnStrike -= ClearHighlight;
            _combat.OnAttackEnded -= ClearHighlight;
        }

        private void HandleWindupStarted(EnemyAttackOption option)
        {
            Color color = _combat.CurrentAttackCanBeParried ? parryableColor : unparryableColor;

            _block.SetColor(BaseColorId, color);
            _block.SetColor(ColorId, color);

            foreach (Renderer target in renderers)
            {
                if (target != null) target.SetPropertyBlock(_block);
            }
        }

        private void ClearHighlight()
        {
            foreach (Renderer target in renderers)
            {
                if (target != null) target.SetPropertyBlock(null);
            }
        }
    }
}
