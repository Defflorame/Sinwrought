using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Stateless-расчёты видимости: множитель зоны, зависимость дальности от угла и прямая видимость.
    /// Туман (RenderSettings.fog) пока не учитывается — здесь будет точка расширения.
    /// Единственное «состояние» — реестр зон, которые сами регистрируются в OnEnable.
    /// </summary>
    public static class VisibilityService
    {
        private static readonly List<VisibilityZone> Zones = new List<VisibilityZone>();

        public static void Register(VisibilityZone zone)
        {
            if (!Zones.Contains(zone)) Zones.Add(zone);
        }

        public static void Unregister(VisibilityZone zone)
        {
            Zones.Remove(zone);
        }

        /// <summary>Множитель дальности зрения в точке (наименьший среди зон, в которых она лежит).</summary>
        public static float GetSightMultiplier(Vector3 position)
        {
            float result = 1f;

            for (int i = 0; i < Zones.Count; i++)
            {
                if (Zones[i].Contains(position))
                {
                    result = Mathf.Min(result, Zones[i].SightMultiplier);
                }
            }

            return result;
        }

        /// <summary>
        /// Доля дальности зрения в зависимости от угла к направлению взгляда (0–180°):
        /// внутри поля зрения — 1, на краю — sideMultiplier, прямо за спиной — rearMultiplier.
        /// </summary>
        public static float GetRangeFactor(float angleFromForward, float fieldOfView, float sideMultiplier, float rearMultiplier)
        {
            float half = Mathf.Clamp(fieldOfView * 0.5f, 0f, 180f);
            if (angleFromForward <= half) return 1f;

            float t = Mathf.InverseLerp(half, 180f, angleFromForward);
            return Mathf.Lerp(sideMultiplier, rearMultiplier, t);
        }

        private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

        /// <summary>
        /// Луч от глаз до цели. true, если между ними нет преград. Коллайдеры самого наблюдателя
        /// (ignoreRoot) и цели (targetRoot) не считаются преградой — иначе враг «заслоняет» обзор сам себе,
        /// особенно когда цель рядом, а глаза выше его собственного коллайдера.
        /// </summary>
        public static bool HasLineOfSight(Vector3 from, Vector3 to, Transform targetRoot, Transform ignoreRoot, LayerMask mask)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            if (distance < 0.01f) return true;

            int count = Physics.RaycastNonAlloc(from, direction / distance, HitBuffer, distance, mask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Transform hit = HitBuffer[i].transform;
                if (ignoreRoot != null && hit.IsChildOf(ignoreRoot)) continue;
                if (hit.IsChildOf(targetRoot)) continue;

                return false;
            }

            return true;
        }
    }
}
