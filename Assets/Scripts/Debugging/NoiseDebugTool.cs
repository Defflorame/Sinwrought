using UnityEngine;
using UnityEngine.InputSystem;
using Game.Enemies;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент. Удалить, когда появится настоящая звуковая система (она вызовет NoiseService.Emit сама).
    /// Клавиша N — шум громкостью loudness в точке noiseOrigin (если не задана — в позиции этого объекта).
    /// </summary>
    public class NoiseDebugTool : MonoBehaviour
    {
        [SerializeField] private Transform noiseOrigin;
        [SerializeField] private float loudness = 15f;

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.nKey.wasPressedThisFrame) return;

            Vector3 position = noiseOrigin != null ? noiseOrigin.position : transform.position;
            Debug.Log($"NoiseDebugTool: шум {loudness} м в {position}");
            NoiseService.Emit(position, loudness, gameObject);
        }
    }
}
