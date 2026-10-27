using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>Линейная ветка: узлы идут сверху вниз, следующий открывается только после предыдущего.</summary>
    [Serializable]
    public class AbilityBranch
    {
        public string branchName;
        public AbilityNodeData[] nodes;
    }

    /// <summary>
    /// Дерево способностей класса: несколько линейных веток. Цена ступени растёт вниз по ветке:
    /// baseCost + номер ступени × costPerTier (отдельный узел можно переопределить costOverride).
    /// Один и тот же узел нельзя использовать в двух местах — он считается одной способностью.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityTree", menuName = "Game/Progression/Ability Tree Data")]
    public class AbilityTreeData : ScriptableObject
    {
        [SerializeField] private AbilityBranch[] branches;

        [Min(0)] public int baseCost = 1;
        [Min(0)] public int costPerTier = 1;

        public IReadOnlyList<AbilityBranch> Branches => branches ?? Array.Empty<AbilityBranch>();

        public AbilityNodeData GetNode(int branchIndex, int nodeIndex)
        {
            if (branches == null || branchIndex < 0 || branchIndex >= branches.Length) return null;

            AbilityBranch branch = branches[branchIndex];
            if (branch?.nodes == null || nodeIndex < 0 || nodeIndex >= branch.nodes.Length) return null;

            return branch.nodes[nodeIndex];
        }

        public int GetCost(int branchIndex, int nodeIndex)
        {
            AbilityNodeData node = GetNode(branchIndex, nodeIndex);
            if (node != null && node.costOverride >= 0) return node.costOverride;

            return baseCost + nodeIndex * costPerTier;
        }

        private void OnValidate()
        {
            if (branches == null) return;

            var seen = new HashSet<AbilityNodeData>();
            foreach (AbilityBranch branch in branches)
            {
                if (branch?.nodes == null) continue;

                foreach (AbilityNodeData node in branch.nodes)
                {
                    if (node != null && !seen.Add(node))
                    {
                        Debug.LogWarning($"AbilityTreeData '{name}': узел '{node.name}' использован больше одного раза.", this);
                    }
                }
            }
        }
    }
}
