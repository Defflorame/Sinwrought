using UnityEngine;
using UnityEngine.InputSystem;
using Game.Combat;
using Game.Player;
using Game.Progression;
using Game.Altars;

namespace Game.Debugging
{
    /// <summary>
    /// ВРЕМЕННЫЙ инструмент (заменяет CharacterStatsDebugTool). Удалить, когда появятся
    /// реальный кристалл (Phase 10), враги с наградой (Phase 8) и LevelUpUI (Phase 14).
    ///
    /// F1 — добавить опыт, F2 — добавить валюту,
    /// F4 — Respawn игрока (воскресить на месте / полное восстановление).
    /// Кнопки в окне вкладывают очки в характеристики и открывают способности через те же публичные
    /// методы, что будет вызывать UI. Мышь: Ctrl включает курсор (DebugCursorToggle).
    /// </summary>
    public class ProgressionDebugOverlay : MonoBehaviour
    {
        [SerializeField] private ProgressionSystem progression;
        [SerializeField] private ProgressionResourceWallet resourceWallet;
        [SerializeField] private CurrencyWallet currencyWallet;
        [SerializeField] private PointSpendingGate gate;
        [SerializeField] private AbilityPointController abilityPoints;
        [SerializeField] private AbilityTreeController abilityTree;

        [Header("Player (Respawn)")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private HealthComponent health;

        [Tooltip("Необязательно. Для строки «последний алтарь».")]
        [SerializeField] private AltarTracker altarTracker;

        [SerializeField] private int experiencePerPress = 100;
        [SerializeField] private int currencyPerPress = 100;

        private Vector2 _scroll;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.f1Key.wasPressedThisFrame) resourceWallet.Add(experiencePerPress);
            if (keyboard.f2Key.wasPressedThisFrame) currencyWallet.Add(currencyPerPress);

            if (keyboard.f4Key.wasPressedThisFrame && playerController != null)
            {
                playerController.HandleRespawn();
            }
        }

        private void OnGUI()
        {
            if (progression == null || resourceWallet == null || currencyWallet == null
                || gate == null || abilityPoints == null || abilityTree == null) return;

            GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            label.normal.textColor = Color.white;
            GUIStyle header = new GUIStyle(label) { fontStyle = FontStyle.Bold };

            const int width = 380;
            const int padding = 10;
            const int top = 400;
            float height = Mathf.Max(200f, Mathf.Min(Screen.height - top - padding, 600f));

            GUI.Box(new Rect(Screen.width - width - padding, top, width, height), "");
            GUILayout.BeginArea(new Rect(Screen.width - width - padding + 10, top + 8, width - 20, height - 16));

            string exp = progression.IsMaxLevel
                ? "MAX"
                : $"{resourceWallet.Amount} / {progression.ExperienceToNextLevel}";

            GUILayout.Label("Progression (F1 опыт, F2 валюта)", header);
            GUILayout.Label($"Class: {(progression.CurrentClass != null ? progression.CurrentClass.className : "-")}   Level: {progression.Level}", label);
            GUILayout.Label($"Exp: {exp}   Currency: {currencyWallet.Amount}", label);
            GUILayout.Label($"Altar gate: {(gate.IsOpen ? "OPEN" : "closed")}", label);
             GUILayout.Label($"Stat points: {progression.UnspentStatPoints}   Ability points: {abilityPoints.AvailablePoints}", label);

            CharacterClassData characterClass = progression.CurrentClass;
            if (characterClass != null)
            {
                int interval = characterClass.abilityPointInterval;
                int nextLevel = progression.Level + (interval - progression.Level % interval);
                GUILayout.Label($"Ability points: +{characterClass.abilityPointsPerInterval} на каждом {interval}-м уровне, следующие на уровне {nextLevel}", label);
            }

            DrawPlayerSection(label, header);

            _scroll = GUILayout.BeginScrollView(_scroll);

            LevelProgressionData rules = progression.CurrentClass != null ? progression.CurrentClass.levelProgression : null;
            if (rules != null)
            {
                GUILayout.Space(4);
                GUILayout.Label("Характеристики", header);

                foreach (StatPointRule rule in rules.StatRules)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{rule.stat}: +{progression.GetAllocatedPoints(rule.stat)}", label, GUILayout.Width(220));
                    GUI.enabled = progression.CanAllocateStatPoint(rule.stat);
                    if (GUILayout.Button("+1", GUILayout.Width(50)))
                    {
                        progression.TryAllocateStatPoint(rule.stat);
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            }

            AbilityTreeData tree = abilityTree.Tree;
            if (tree != null)
            {
                GUILayout.Space(6);
                GUILayout.Label("Способности", header);

                for (int b = 0; b < tree.Branches.Count; b++)
                {
                    AbilityBranch branch = tree.Branches[b];
                    GUILayout.Label(branch.branchName, header);
                    if (branch.nodes == null) continue;

                    for (int n = 0; n < branch.nodes.Length; n++)
                    {
                        AbilityNodeData node = branch.nodes[n];
                        if (node == null) continue;

                        GUILayout.BeginHorizontal();
                        string state = abilityTree.IsUnlocked(node) ? "[открыто]" : $"[{tree.GetCost(b, n)}]";
                        GUILayout.Label($"{node.abilityName} {state}", label, GUILayout.Width(260));
                        GUI.enabled = abilityTree.CanUnlock(b, n);
                        if (GUILayout.Button("Open", GUILayout.Width(60)))
                        {
                            abilityTree.TryUnlock(b, n);
                        }
                        GUI.enabled = true;
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(6);
                GUI.enabled = gate.IsOpen;
                if (GUILayout.Button("Сбросить дерево"))
                {
                    abilityTree.TryReset();
                }
                GUI.enabled = true;
            }
            else
            {
                GUILayout.Space(6);
                GUILayout.Label("Дерево не назначено в CharacterClassData (поле Ability Tree).", label);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawPlayerSection(GUIStyle label, GUIStyle header)
        {
            if (playerController == null || health == null) return;

            GUILayout.Space(4);
            GUILayout.Label("Player (F4 respawn)", header);
            GUILayout.Label($"HP: {health.CurrentHealth} / {health.MaxHealth}  ({(health.IsDead() ? "DEAD" : "alive")})", label);

            if (altarTracker != null)
            {
                string last = altarTracker.LastAltar != null ? altarTracker.LastAltar.AltarName : "-";
                GUILayout.Label($"Last altar: {last}", label);
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !health.IsDead();
            if (GUILayout.Button("Kill"))
            {
                health.Kill();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Respawn"))
            {
                playerController.HandleRespawn();
            }
            GUILayout.EndHorizontal();
        }
    }
}