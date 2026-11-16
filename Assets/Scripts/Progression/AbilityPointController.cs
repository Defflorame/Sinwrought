using System;
using Game.Save;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Очки способностей: выдаёт их за уровни по правилу класса (каждый N-й уровень)
    /// и тратит/возвращает по запросу AbilityTreeController. О дереве ничего не знает.
    /// </summary>
    public class AbilityPointController : MonoBehaviour, ISaveable
    {
        [Serializable]
        private class PointsSaveData
        {
            public int points;
        }
        [SerializeField] private ProgressionSystem progression;

        public int AvailablePoints { get; private set; }

        public event Action<int> OnPointsChanged;
        public string SaveKey => "abilityPoints";
        public int RestoreOrder => 42;

        public string Capture()
        {
            return JsonUtility.ToJson(new PointsSaveData { points = AvailablePoints });
        }

        public void Restore(string json)
        {
            PointsSaveData data = JsonUtility.FromJson<PointsSaveData>(json);
            if (data == null) return;

            AvailablePoints = Mathf.Max(0, data.points);
            OnPointsChanged?.Invoke(AvailablePoints);
        }
        private void OnEnable()
        {
            progression.OnLevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            progression.OnLevelChanged -= HandleLevelChanged;
        }

        public bool CanSpend(int amount) => amount >= 0 && AvailablePoints >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount)) return false;

            AvailablePoints -= amount;
            OnPointsChanged?.Invoke(AvailablePoints);
            return true;
        }

        public void Refund(int amount)
        {
            if (amount <= 0) return;

            AvailablePoints += amount;
            OnPointsChanged?.Invoke(AvailablePoints);
        }

        private void HandleLevelChanged(int newLevel)
        {
            CharacterClassData characterClass = progression.CurrentClass;
            if (characterClass == null) return;

            int points = characterClass.GetAbilityPointsForLevel(newLevel);
            if (points <= 0) return;

            AvailablePoints += points;
            Debug.Log($"AbilityPointController: +{points} очков способностей (всего {AvailablePoints})");
            OnPointsChanged?.Invoke(AvailablePoints);
        }
    }
}
