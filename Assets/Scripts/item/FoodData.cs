using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Inventory/Food")]
public class FoodData : ItemData
{
    [Header("������� ���")]
    public int hungerRestored;   // ������� ��������������� �������
    public int energyRestored;   // ������� ��������������� �������
}
