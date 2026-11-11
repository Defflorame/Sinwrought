namespace Game.Enemies
{
    /// <summary>
    /// Настройка КОНКРЕТНОГО экземпляра врага на сцене (EnemyController), а не префаба/EnemyData.
    /// </summary>
    public enum EnemyPursuitMode
    {
        /// <summary>Обычное поведение: потерял цель — идёт в Investigate, потом возвращается.</summary>
        Normal,

        /// <summary>После первого обнаружения никогда не теряет цель (преследует вечно).</summary>
        NeverLoseTarget,

        /// <summary>Знает, где игрок, с самого начала и всегда преследует.</summary>
        AlwaysHunting
    }
}
