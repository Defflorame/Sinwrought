using UnityEngine;

namespace Game.Items
{
    /// <summary>Сюжетный предмет (ключи и т.п.). Отдельный тип нужен, чтобы инвентарь мог разделять категории (раздел 36).</summary>
    [CreateAssetMenu(fileName = "NewQuestItemData", menuName = "Game/Items/Quest Item Data")]
    public class QuestItemData : ItemData { }
}