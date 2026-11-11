using UnityEngine;

namespace Game.Combat
{
    /// <summary>Проверка «входит ли слой объекта в LayerMask». Нужна и оружию, и снарядам заклинаний.</summary>
    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}