// Game.Save/ISaveable.cs
namespace Game.Save
{
    /// <summary>
    /// Компонент, который сам знает свою секцию сохранения. SaveController собирает такие
    /// компоненты через интерфейс и не знает их типов. Restore заменяет состояние целиком
    /// (вызывается на живом игроке), не вызывает Consume и не проверяет требования.
    /// </summary>
    public interface ISaveable
    {
        string SaveKey { get; }

        /// <summary>Меньше — раньше. Нужен, когда одна секция зависит от другой.</summary>
        int RestoreOrder { get; }

        string Capture();

        void Restore(string json);
    }
}