namespace Game.Enemies
{
    /// <summary>Состояние EnemyStateMachine. Прервать любое состояние (кроме смерти) может EnemyBrain.</summary>
    public interface IEnemyState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}
