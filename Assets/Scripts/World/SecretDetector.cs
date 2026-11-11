using UnityEngine;

namespace Game.World
{
    /// <summary>Раскрывает иллюзорные стены вокруг. Активируется вызовом Activate() (позже — предметом).</summary>
    public class SecretDetector : MonoBehaviour
    {
        [SerializeField] private float radius = 8f;
        [SerializeField] private float revealDuration = 6f;

        public void Activate()
        {
            Vector3 origin = transform.position;
            float sqrRadius = radius * radius;
            int revealed = 0;

            var walls = IllusoryWall.All;
            for (int i = 0; i < walls.Count; i++)
            {
                if ((walls[i].transform.position - origin).sqrMagnitude > sqrRadius) continue;

                walls[i].Reveal(revealDuration);
                revealed++;
            }

            Debug.Log($"SecretDetector: раскрыто стен — {revealed}");
        }
    }
}