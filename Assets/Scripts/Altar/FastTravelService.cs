using UnityEngine;
using Game.World;

namespace Game.Altars
{
    /// <summary>
    /// Правила fast travel (stateless). Условие разблокировки проверяется через IsMet
    /// и ничего не расходует: после выполнения флаг запоминается в AltarTracker навсегда.
    /// </summary>
    public static class FastTravelService
    {
        /// <summary>Пустой список условий = условий нет (алтарь доступен сразу).</summary>
        public static bool IsUnlockConditionMet(AltarData altar, GameObject owner)
        {
            return altar != null && AccessRequirement.AreAllMet(altar.FastTravelRequirements, owner);
        }

        /// <summary>Ехать можно только с разблокированного алтаря на другой разблокированный.</summary>
        public static bool CanTravel(AltarData from, AltarData to, AltarTracker tracker)
        {
            if (from == null || to == null || tracker == null || from == to) return false;
            if (to.Location == null || to.SpawnPoint == null) return false;

            return tracker.IsFastTravelUnlocked(from) && tracker.IsFastTravelUnlocked(to);
        }
    }
}