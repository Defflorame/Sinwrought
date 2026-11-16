using System.Collections.Generic;
using UnityEngine;
using Game.Items;

namespace Game.World
{
    /// <summary>
    /// Выбрасывает мешки с лутом из точки на объекте (враг). По мешку на каждый предмет; мешки разлетаются
    /// веером с небольшой скоростью, чтобы был виден и труп, и добыча. Бросок таблицы не делает: ему
    /// передают готовый результат LootService.Roll.
    /// </summary>
    public class LootDropper : MonoBehaviour
    {
        [SerializeField] private DroppedLoot lootPrefab;

        [Tooltip("Откуда вылетают мешки относительно корня объекта.")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.8f, 0f);

        [Min(0f)]
        [Tooltip("Скорость разлёта в стороны, м/с.")]
        [SerializeField] private float horizontalSpeed = 1.5f;

        [Min(0f)]
        [Tooltip("Начальная скорость вверх, м/с.")]
        [SerializeField] private float upwardSpeed = 2.5f;

        public void Drop(IReadOnlyList<LootDrop> drops)
        {
            if (drops == null || drops.Count == 0) return;

            if (lootPrefab == null)
            {
                Debug.LogWarning($"LootDropper: у {name} не назначен префаб мешка — лут потерян.", this);
                return;
            }

            Vector3 origin = transform.position + spawnOffset;
            float step = 360f / drops.Count;
            float startAngle = Random.Range(0f, 360f);

            for (int i = 0; i < drops.Count; i++)
            {
                // Равномерный веер со случайным сдвигом, чтобы мешки не лежали ровной звездой.
                float angle = (startAngle + step * i + Random.Range(-step * 0.25f, step * 0.25f)) * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                float speed = horizontalSpeed * Random.Range(0.7f, 1.3f);

                DroppedLoot loot = Instantiate(lootPrefab, origin, Random.rotation);
                loot.Initialize(drops[i].Item, drops[i].Quantity, direction * speed + Vector3.up * upwardSpeed);
            }
        }
    }
}