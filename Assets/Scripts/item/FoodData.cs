using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Inventory/Food")]
public class FoodData : ItemData
{
    [Header("Ёффекты еды")]
    public int hungerRestored;   // сколько восстанавливает сытость
    public int energyRestored;   // сколько восстанавливает энергию
}
