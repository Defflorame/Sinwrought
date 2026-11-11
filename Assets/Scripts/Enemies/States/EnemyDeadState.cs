using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Смерть. Отключает ИИ и коллайдеры. Если задан corpsePrefab — заменяет врага трупом
    /// (пепел, кости), который исчезает через corpseLifetime; иначе враг остаётся лежать сам.
    /// Награду выдаёт RewardDispenser по событию OnDeath — не здесь.
    /// </summary>
    public class EnemyDeadState : IEnemyState
    {
        private readonly EnemyContext _ctx;

        public EnemyDeadState(EnemyContext ctx)
        {
            _ctx = ctx;
        }

        public void Enter()
        {
            _ctx.Combat.CancelAttack();
            _ctx.Movement.Disable();
            _ctx.Perception.enabled = false;
            _ctx.Combat.enabled = false;

            foreach (Collider collider in _ctx.Self.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            EnemyData data = _ctx.Data;

            if (data.corpsePrefab != null)
            {
                Vector3 position = _ctx.Self.position - Vector3.up * data.hoverHeight;
                GameObject corpse = Object.Instantiate(data.corpsePrefab, position, _ctx.Self.rotation);
                Object.Destroy(corpse, data.corpseLifetime);
                Object.Destroy(_ctx.Self.gameObject);
            }
            else
            {
                Object.Destroy(_ctx.Self.gameObject, data.corpseLifetime);
            }
        }

        public void Tick()
        {
        }

        public void Exit()
        {
        }
    }
}
