using UnityEngine;
using Game.Items;

namespace Game.World
{
    /// <summary>
    /// Сундук = WorldGate + этот opener. Открывает ворота и выдаёт содержимое: награду выдаёт именно opener,
    /// потому что только он знает, кто открыл. Запертый сундук получается требованием в WorldGate (ключ).
    /// Открытое состояние хранит PersistentGateState; после загрузки RestoreState не вызывает открытие,
    /// поэтому содержимое повторно не выдаётся.
    /// </summary>
    public class ChestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private WorldGate gate;
        [SerializeField] private DropTableData loot;
        [SerializeField] private string prompt = "Открыть";

        public bool CanInteract(GameObject interactor) => gate != null && !gate.IsOpen;

        public void Interact(GameObject interactor)
        {
            if (gate == null || gate.IsOpen) return;

            // Инвентарь проверяем до открытия, чтобы не потратить ключ впустую.
            Inventory inventory = interactor.GetComponentInChildren<Inventory>();
            if (inventory == null)
            {
                Debug.LogWarning("ChestInteractable: у игрока нет Inventory.", this);
                return;
            }

            if (!gate.TryOpen(interactor)) return;

            LootService.Grant(loot, inventory);
        }

        public string GetInteractionPrompt() => prompt;

        private void OnValidate()
        {
            if (loot == null) return;

            // Шанс < 100% в сундуке позволяет перезагружать сохранение до нужного результата.
            foreach (DropEntry entry in loot.Entries)
            {
                if (entry != null && entry.chancePercent < 100f)
                {
                    Debug.LogWarning($"ChestInteractable: в таблице '{loot.name}' у '{(entry.item != null ? entry.item.name : "пусто")}' шанс меньше 100% — возможен сейвскам.", this);
                }
            }
        }
    }
}