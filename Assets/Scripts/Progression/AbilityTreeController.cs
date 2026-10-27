using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Состояние дерева способностей: какие узлы открыты. Открытие и сброс — только пока открыт
    /// PointSpendingGate. Эффекты узлов применяются к объекту, на котором висит контроллер (игрок).
    /// Узел открывается, только если открыт предыдущий узел ветки и хватает очков.
    /// Какие эффекты бывают, контроллер не знает: он вызывает AbilityEffect.Apply/Remove.
    /// </summary>
    public class AbilityTreeController : MonoBehaviour
    {
        [SerializeField] private ProgressionSystem progression;
        [SerializeField] private AbilityPointController points;
        [SerializeField] private PointSpendingGate gate;

        // узел → сколько очков за него заплачено (для возврата при сбросе)
        private readonly Dictionary<AbilityNodeData, int> _unlocked = new Dictionary<AbilityNodeData, int>();

        public AbilityTreeData Tree => progression.CurrentClass != null ? progression.CurrentClass.abilityTree : null;

        public event Action<AbilityNodeData> OnNodeUnlocked;
        public event Action OnTreeReset;

        public bool IsUnlocked(AbilityNodeData node) => node != null && _unlocked.ContainsKey(node);

        public bool CanUnlock(int branchIndex, int nodeIndex)
        {
            AbilityTreeData tree = Tree;
            if (tree == null || gate == null || !gate.IsOpen) return false;

            AbilityNodeData node = tree.GetNode(branchIndex, nodeIndex);
            if (node == null || IsUnlocked(node)) return false;

            if (nodeIndex > 0 && !IsUnlocked(tree.GetNode(branchIndex, nodeIndex - 1))) return false;

            return points.CanSpend(tree.GetCost(branchIndex, nodeIndex));
        }

        public bool TryUnlock(int branchIndex, int nodeIndex)
        {
            if (!CanUnlock(branchIndex, nodeIndex)) return false;

            AbilityTreeData tree = Tree;
            AbilityNodeData node = tree.GetNode(branchIndex, nodeIndex);
            int cost = tree.GetCost(branchIndex, nodeIndex);

            if (!points.TrySpend(cost)) return false;

            _unlocked[node] = cost;
            ApplyEffects(node);

            Debug.Log($"AbilityTreeController: открыта способность '{node.abilityName}' за {cost}");
            OnNodeUnlocked?.Invoke(node);
            return true;
        }

        /// <summary>Снимает все способности и возвращает потраченные очки. Только у кристалла.</summary>
        public bool TryReset()
        {
            if (gate == null || !gate.IsOpen || _unlocked.Count == 0) return false;

            AbilityContext context = new AbilityContext(gameObject);
            int refund = 0;

            foreach (KeyValuePair<AbilityNodeData, int> entry in _unlocked)
            {
                refund += entry.Value;

                if (entry.Key.effects == null) continue;
                foreach (AbilityEffect effect in entry.Key.effects)
                {
                    if (effect != null) effect.Remove(context, entry.Key);
                }
            }

            _unlocked.Clear();
            points.Refund(refund);

            Debug.Log($"AbilityTreeController: дерево сброшено, возвращено {refund} очков");
            OnTreeReset?.Invoke();
            return true;
        }

        private void ApplyEffects(AbilityNodeData node)
        {
            if (node.effects == null) return;

            AbilityContext context = new AbilityContext(gameObject);
            foreach (AbilityEffect effect in node.effects)
            {
                if (effect != null) effect.Apply(context, node);
            }
        }
    }
}
