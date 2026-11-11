using UnityEngine;
using UnityEngine.InputSystem;
using Game.World;

namespace Game.Debugging
{
    /// <summary>ВРЕМЕННЫЙ инструмент. H — активировать SecretDetector. Удалить, когда появится предмет-активатор.</summary>
    public class SecretDetectorDebugTool : MonoBehaviour
    {
        [SerializeField] private SecretDetector detector;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            {
                detector.Activate();
            }
        }
    }
}