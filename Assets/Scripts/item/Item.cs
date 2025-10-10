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

    // ?? ��������� ��� ScriptableObject-������
    public ItemData data;

    // ?? ���������� ������� � �� ������ ������ ���
    public string itemName => data != null ? data.itemName : name;
    public Sprite itemIcon => data != null ? data.icon : icon;
    public ItemType itemType => data != null ? data.type : type;
    public int itemValue => data != null ? data.value : value;
}

// ?? ���� enum ����������� ������ ���� � ���� �� �����!
public enum ItemType
{
    Weapon,
    Consumable,
    Resource,
    Tool,
    Armor,
    Food // ? ����� ����� �������� ��� ���, ���� �����
}
