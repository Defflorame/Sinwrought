using UnityEngine;
using Game.Combat;

namespace Game.Magic
{
    /// <summary>
    /// Снаряд заклинания. Урон не считает: попадание отдаётся SpellHitResolver.
    /// Параметры (скорость, время жизни, радиус взрыва) читаются из ProjectileEffectData.
    /// Префаб: коллайдер + Rigidbody + этот скрипт (isTrigger/kinematic выставляются в Awake).
    /// Если explosionRadius > 0 — при попадании (в цель ИЛИ в стену) бьёт по области вокруг снаряда.
    /// Область взрыва видна в Scene: оранжевая сфера у летящего снаряда (Gizmos)
    /// и красная вспышка на 1 секунду в точке самого взрыва (SpellDebugDraw).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class SpellProjectile : MonoBehaviour
    {
        private const float ExplosionDebugDuration = 1f;

        private ProjectileEffectData _data;
        private SpellData _spell;
        private GameObject _caster;
        private LayerMask _hitMask;
        private int _damage;
        private Rigidbody _body;
        private bool _hasHit;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.isKinematic = true;
            _body.useGravity = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            GetComponent<Collider>().isTrigger = true;
        }

        public void Initialize(ProjectileEffectData data, SpellCastContext context, int damage)
        {
            _data = data;
            _spell = context.Spell;
            _caster = context.Caster;
            _hitMask = context.AimMask;
            _damage = damage;
            Destroy(gameObject, data.lifetime);
        }

        private void FixedUpdate()
        {
            if (_data == null) return;

            _body.MovePosition(_body.position + transform.forward * (_data.speed * Time.fixedDeltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit || _data == null) return;
            if (SpellHitResolver.IsCaster(_caster, other)) return;
            if (!_hitMask.Contains(other.gameObject.layer)) return;

            bool reactive = other.GetComponentInParent<DamageReceiver>() != null
                || other.GetComponentInParent<SpellReceiver>() != null;

            // Чужие триггеры (хитбоксы и т.п.), с которыми нечего делать, игнорируем.
            if (!reactive && other.isTrigger) return;

            _hasHit = true;

            if (_data.explosionRadius > 0f)
            {
                SpellDebugDraw.Sphere(transform.position, _data.explosionRadius, Color.red, ExplosionDebugDuration);

                SpellHitResolver.HitArea(
                    _spell, _caster, transform.position, _data.explosionRadius,
                    _damage, _data.canBeBlocked, _data.explosionAffectsCaster, _hitMask);
            }
            else if (reactive)
            {
                SpellHitResolver.HitCollider(_spell, _caster, other, _damage, _data.canBeBlocked);
            }

            Destroy(gameObject);
        }

        /// <summary>Радиус взрыва вокруг летящего снаряда. Данные известны только после Initialize — то есть в Play Mode.</summary>
        private void OnDrawGizmos()
        {
            if (_data == null || _data.explosionRadius <= 0f) return;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, _data.explosionRadius);
        }
    }
}
