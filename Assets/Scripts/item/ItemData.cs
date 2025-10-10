using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("�������� ���������")]
    public string itemName;
    public Sprite icon;
    public ItemType type;      // Weapon, Consumable, Resource, Tool, Armor
    public int value;          // ������� �������� (����, ������� � �.�.)

    [Header("����")]
    [Tooltip("������������ ���������� ��������� � ����� ����� ���������")]
    public int maxStack = 10;  // <-- �������� ��� ����

    [Header("��������� ��� (���� Consumable)")]
    [Tooltip("��������������� ���� ������")]
    public int hungerRestore;
    [Tooltip("��������������� �������")]
    public int energyRestore;
    [Tooltip("��������������� �������� (�������� ���� ��� ����)")]
    public int healthRestore;

    [Header("����������� ������� (���� ������� ����� ������� �� ��������)")]
    public GameObject placeablePrefab; // <-- ����� field, ��������� ��������
}
