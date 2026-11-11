using UnityEngine;

namespace Game.World
{
    /// <summary>Единственное место с именами параметров Animator ворот. Имена нужны и Editor-скрипту, который собирает контроллер.</summary>
    public static class GateAnimatorIds
    {
        public const string IsOpenName = "IsOpen";
        public static readonly int IsOpen = Animator.StringToHash(IsOpenName);

        /// <summary>Float 0..1: положение створки. Подключён как Motion Time единственного состояния.</summary>
        public const string ProgressName = "Progress";
        public static readonly int Progress = Animator.StringToHash(ProgressName);
    }
}