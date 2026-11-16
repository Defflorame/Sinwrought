using System;
using System.Collections.Generic;
using Game.Save;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Состояние дерева способностей: какие узлы открыты. Открытие и сброс — только пока открыт
    /// PointSpendingGate. Эффекты узлов применяются к объекту, на котором висит контроллер (игрок).
    /// Узел открывается, только если открыт предыдущий узел ветки и хватает очков.
    /// Какие эффекты бывают, контроллер не знает: он вызывает AbilityEffect.Apply/Remove.
    /// </summary>
    public class AbilityTreeController : MonoBehaviour, ISaveable
    {
        [Serializable]
        private class UnlockedNodeData
        {
            public string nodeId;
            public int paidCost;
        }

        [Serializable]
        private class TreeSaveData
        {
            public List<UnlockedNodeData> nodes = new List<UnlockedNodeData>();
        }

        [SerializeField] private AbilityNodeCatalogData nodeCatalog;
        [SerializeField] private ProgressionSystem progression;
        [SerializeField] private AbilityPointController points;
        [SerializeField] private PointSpendingGate gate;

        // узел → сколько очков за него заплачено (для возврата при сбросе)
        private readonly Dictionary<AbilityNodeData, int> _unlocked = new Dictionary<AbilityNodeData, int>();

        public AbilityTreeData Tree => progression.CurrentClass != null ? progression.CurrentClass.abilityTree : null;

        public event Action<AbilityNodeData> OnNodeUnlocked;
        public event Action OnTreeReset;

        public event Action OnTreeRestored;

        public string SaveKey => "abilityTree";
        public int RestoreOrder => 43;

        public string Capture()
        {
            var data = new TreeSaveData();
            foreach (KeyValuePair<AbilityNodeData, int> entry in _unlocked)
            {
                data.nodes.Add(new UnlockedNodeData { nodeId = entry.Key.Id, paidCost = entry.Value });
            }

            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Заменяет набор открытых узлов. Не вызывает TryUnlock: очки не тратятся, gate не нужен.
        /// Сначала снимаются эффекты текущих узлов, затем применяются сохранённые.
        /// </summary>
        public void Restore(string json)
        {
            TreeSaveData data = JsonUtility.FromJson<TreeSaveData>(json);
            if (data == null) return;

            AbilityContext context = new AbilityContext(gameObject);
            foreach (KeyValuePair<AbilityNodeData, int> entry in _unlocked)
            {
                RemoveEffects(context, entry.Key);
            }
            _unlocked.Clear();

            foreach (UnlockedNodeData saved in data.nodes)
            {
                if (nodeCatalog == null || !nodeCatalog.TryGet(saved.nodeId, out AbilityNodeData node))
                {
                    Debug.LogWarning($"AbilityTreeController: узел '{saved.nodeId}' не найден в каталоге, пропущен.", this);
                    continue;
                }

                if (_unlocked.ContainsKey(node)) continue;

                _unlocked[node] = Mathf.Max(0, saved.paidCost);
                ApplyEffects(node);
            }

            OnTreeRestored?.Invoke();
        }

        // TryReset: вместо внутреннего цикла по effects одна строка
        public bool TryReset()
        {
            if (gate == null || !gate.IsOpen || _unlocked.Count == 0) return false;

            AbilityContext context = new AbilityContext(gameObject);
            int refund = 0;

            foreach (KeyValuePair<AbilityNodeData, int> entry in _unlocked)
            {
                refund += entry.Value;
                RemoveEffects(context, entry.Key);
            }

            _unlocked.Clear();
            points.Refund(refund);

            Debug.Log($"AbilityTreeController: дерево сброшено, возвращено {refund} очков");
            OnTreeReset?.Invoke();
            return true;
        }

        private void RemoveEffects(AbilityContext context, AbilityNodeData node)
        {
            if (node.effects == null) return;

            foreach (AbilityEffect effect in node.effects)
            {
                if (effect != null) effect.Remove(context, node);
            }
        }

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
