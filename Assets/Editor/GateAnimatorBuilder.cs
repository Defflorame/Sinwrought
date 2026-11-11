using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Game.World;

namespace Game.EditorTools
{
    /// <summary>
    /// Создаёт Gate_Base.controller (три состояния на одном клипе: Closed / Opening / Closing),
    /// заготовки клипов и Override-контроллеры для типов дверей. Запускать один раз.
    /// Существующие ассеты не перезаписывает. Заготовки клипов — просто пример: настройте ключи в окне Animation.
    /// Все клипы рассчитаны на объект Mover с нулевой локальной позой (см. описание в чате).
    /// </summary>
    public static class GateAnimatorBuilder
    {
        private const string RootFolder = "Assets/Animations/Gates";
        private const string ClipsFolder = RootFolder + "/Clips";
        private const string BaseClipName = "GateOpen";

        private enum Channel
        {
            Position,
            Euler,
            Scale
        }

        [MenuItem("Game/Animations/Create Gate Controllers")]
        public static void Build()
        {
            EnsureFolders();

            AnimationClip baseClip = GetOrCreateClip(BaseClipName, Channel.Position,
                (0f, Vector3.zero), (1f, new Vector3(0f, 2f, 0f)));

            AnimatorController baseController = GetOrCreateBaseController(baseClip);
            if (baseController == null) return;

            // Заготовки типов дверей. Поменяйте ключи в окне Animation под свои размеры.
            CreateOverride(baseController, baseClip, "Gate_SecretDoor", GetOrCreateClip("GateOpen_SecretDoor", Channel.Position,
                (0f, Vector3.zero), (0.4f, new Vector3(-0.2f, 0f, 0f)), (1.2f, new Vector3(-0.2f, 2.2f, 0f))));

            CreateOverride(baseController, baseClip, "Gate_SideDoor", GetOrCreateClip("GateOpen_SideDoor", Channel.Position,
                (0f, Vector3.zero), (0.8f, new Vector3(1.5f, 0f, 0f))));

            CreateOverride(baseController, baseClip, "Gate_Lever", GetOrCreateClip("GateOpen_Lever", Channel.Euler,
                (0f, Vector3.zero), (0.3f, new Vector3(-60f, 0f, 0f))));

            CreateOverride(baseController, baseClip, "Gate_Roots", GetOrCreateClip("GateOpen_Roots", Channel.Scale,
                (0f, Vector3.one), (1.2f, Vector3.zero)));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = baseController;
            Debug.Log($"GateAnimatorBuilder: готово, ассеты в {RootFolder}");
        }

        private static AnimatorController GetOrCreateBaseController(AnimationClip baseClip)
        {
            string path = $"{RootFolder}/Gate_Base.controller";

            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null)
            {
                Debug.LogWarning($"GateAnimatorBuilder: {path} уже существует — не перезаписываю, использую его для Override-контроллеров.");
                return existing;
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter(GateAnimatorIds.IsOpenName, AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // Все три состояния играют один и тот же клип: на месте (скорость 0), вперёд и назад.
            AnimatorState closed = AddState(sm, "Closed", baseClip, 0f);
            sm.defaultState = closed;
            AnimatorState opening = AddState(sm, "Opening", baseClip, 1f);
            AnimatorState closing = AddState(sm, "Closing", baseClip, -1f);

            // Closed → Opening: сразу.
            Link(closed, opening, hasExitTime: false, offset: 0f, isOpen: true);
            // Opening → Closing: только после окончания открытия (запрос закрыть ждёт в очереди), старт обратного клипа с конца.
            Link(opening, closing, hasExitTime: true, offset: 1f, isOpen: false);
            // Closing → Opening: сразу (без Exit Time: при отрицательной скорости он неоднозначен).
            Link(closing, opening, hasExitTime: false, offset: 0f, isOpen: true);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine sm, string stateName, AnimationClip clip, float speed)
        {
            AnimatorState state = sm.AddState(stateName);
            state.motion = clip;
            state.speed = speed;
            return state;
        }

        private static void Link(AnimatorState from, AnimatorState to, bool hasExitTime, float offset, bool isOpen)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.exitTime = 1f;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            transition.offset = offset;
            transition.AddCondition(isOpen ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, GateAnimatorIds.IsOpenName);
        }

        private static void CreateOverride(AnimatorController baseController, AnimationClip baseClip, string overrideName, AnimationClip clip)
        {
            string path = $"{RootFolder}/{overrideName}.overrideController";
            if (AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path) != null) return;

            var overrideController = new AnimatorOverrideController(baseController);
            overrideController[baseClip] = clip;
            AssetDatabase.CreateAsset(overrideController, path);
        }

        private static AnimationClip GetOrCreateClip(string clipName, Channel channel, params (float time, Vector3 value)[] keys)
        {
            string path = $"{ClipsFolder}/{clipName}.anim";
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null) return existing;

            var clip = new AnimationClip { name = clipName };

            string property = channel == Channel.Position ? "m_LocalPosition"
                : channel == Channel.Euler ? "localEulerAnglesRaw"
                : "m_LocalScale";

            string[] axes = { ".x", ".y", ".z" };
            for (int axis = 0; axis < 3; axis++)
            {
                var curve = new AnimationCurve();
                foreach ((float time, Vector3 value) in keys)
                {
                    curve.AddKey(time, value[axis]);
                }

                for (int i = 0; i < curve.length; i++)
                {
                    curve.SmoothTangents(i, 0f);
                }

                EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Transform), property + axes[axis]);
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            // Клипы ворот не зациклены: последний кадр держит открытую позу.
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
            if (!AssetDatabase.IsValidFolder(RootFolder)) AssetDatabase.CreateFolder("Assets/Animations", "Gates");
            if (!AssetDatabase.IsValidFolder(ClipsFolder)) AssetDatabase.CreateFolder(RootFolder, "Clips");
        }
    }
}