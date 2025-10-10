using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;
    public Sprite icon;
    public ItemType type;
    public int value;
    public int amount = 1;
    public int maxStack = 10;

    // ?? ƒобавлено дл€ ScriptableObject-данных
    public ItemData data;

    // ?? Ѕезопасные геттеры Ч не ломают старый код
    public string itemName => data != null ? data.itemName : name;
    public Sprite itemIcon => data != null ? data.icon : icon;
    public ItemType itemType => data != null ? data.type : type;
    public int itemValue => data != null ? data.value : value;
}

// ?? Ётот enum об€зательно должен быть в этом же файле!
public enum ItemType
{
    Weapon,
    Consumable,
    Resource,
    Tool,
    Armor,
    Food // ? можно сразу добавить тип еды, если нужно
}
