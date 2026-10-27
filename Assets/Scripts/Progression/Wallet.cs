using System;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Общая логика кошелька: целое количество чего-либо, добавить / потратить, событие изменения.
    /// Сам по себе не используется: ProgressionResourceWallet (опыт) и CurrencyWallet (деньги) —
    /// отдельные типы, чтобы их нельзя было перепутать или объединить (раздел 47 документа).
    /// Сохранение (Phase 11) добавит сюда загрузку значения вместе с ISaveable.
    /// </summary>
    public abstract class Wallet : MonoBehaviour
    {
        public int Amount { get; private set; }

        /// <summary>(новое значение, изменение). Изменение отрицательное, если потратили.</summary>
        public event Action<int, int> OnChanged;

        public void Add(int amount)
        {
            if (amount <= 0) return;

            int newAmount = (int)Math.Min((long)Amount + amount, int.MaxValue);
            int delta = newAmount - Amount;
            if (delta == 0) return;

            Amount = newAmount;
            OnChanged?.Invoke(Amount, delta);
        }

        public bool CanSpend(int amount)
        {
            return amount >= 0 && Amount >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount)) return false;
            if (amount == 0) return true;

            Amount -= amount;
            OnChanged?.Invoke(Amount, -amount);
            return true;
        }
    }
}
