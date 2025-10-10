using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Основные параметры")]
    public string itemName;
    public Sprite icon;
    public ItemType type;      // Weapon, Consumable, Resource, Tool, Armor
    public int value;          // базовое значение (урон, лечение и т.д.)

    [Header("Стак")]
    [Tooltip("Максимальное количество предметов в одном слоте инвентаря")]
    public int maxStack = 10;  // <-- добавили это поле

    [Header("Параметры еды (если Consumable)")]
    [Tooltip("Восстанавливает очки голода")]
    public int hungerRestore;
    [Tooltip("Восстанавливает энергию")]
    public int energyRestore;
    [Tooltip("Восстанавливает здоровье (например бинт или мясо)")]
    public int healthRestore;

    [Header("Размещаемые объекты (если предмет можно ставить на террейне)")]
    public GameObject placeablePrefab; // <-- новый field, безопасно добавлен
}
