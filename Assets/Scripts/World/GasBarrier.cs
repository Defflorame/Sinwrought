using UnityEngine;
using Game.Player;

namespace Game.World
{
    /// <summary>
    /// Газовая зона: твёрдый барьер пропускает игрока, у которого выполнены требования (например, надет аксессуар).
    /// Висит на объекте с коллайдером-ТРИГГЕРОМ, который чуть больше барьера, чтобы игрок вошёл в зону раньше, чем упрётся в твёрдый коллайдер.
    /// Проверка только при входе: если снять аксессуар внутри зоны, барьер вернётся после выхода.
    /// Требования не расходуются.
    /// </summary>
    public class GasBarrier : MonoBehaviour
    {
        [SerializeField] private AccessRequirement[] requirements;
        [Tooltip("Твёрдый (не триггер) коллайдер барьера. Дочерний объект, а не этот.")]
        [SerializeField] private Collider solidBarrier;

        private void Awake()
        {
            Collider zone = GetComponent<Collider>();
            if (zone == null || !zone.isTrigger)
            {
                Debug.LogWarning($"GasBarrier: на {name} нужен коллайдер с включённым Is Trigger.", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            if (AccessRequirement.AreAllMet(requirements, player.gameObject))
            {
                solidBarrier.enabled = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;

            solidBarrier.enabled = true;
        }
    }
}