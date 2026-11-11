using UnityEngine;
using Game.Combat;
using Game.Stats;


namespace Game.Enemies
{
    /// <summary>
    /// Всё, что нужно EnemyBrain и состояниям: ссылки на компоненты и данные конкретного врага.
    /// Обычный класс без логики: собирает EnemyController, чтобы Brain и состояния не зависели от него.
    /// </summary>
    public class EnemyContext
    {
        public EnemyData Data;
        public Transform Self;
        public Transform Target;
        public HealthComponent TargetHealth;
        public Vector3 HomePosition;

        public EnemyPursuitMode Pursuit;
        public Transform[] Waypoints;
        public bool PingPongPatrol;

        public EnemyMovement Movement;
        public EnemyPerception Perception;
        public EnemyCombatController Combat;
        public DamageReceiver Receiver;
        public HealthComponent Health;
        public CharacterStats Stats;

        public bool HasTarget => Target != null;
        public bool TargetIsDead => TargetHealth != null && TargetHealth.IsDead();

        public float FlatDistanceToTarget
        {
            get
            {
                Vector3 delta = Target.position - Self.position;
                delta.y = 0f;
                return delta.magnitude;
            }
        }
    }
}
