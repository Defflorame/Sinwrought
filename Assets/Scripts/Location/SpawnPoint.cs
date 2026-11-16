using UnityEngine;

namespace Game.Locations
{
    /// <summary>
    /// Точка появления игрока в сцене. Позиция и направление берутся из transform.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpawnPointData data;

        public SpawnPointData Data => data;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}