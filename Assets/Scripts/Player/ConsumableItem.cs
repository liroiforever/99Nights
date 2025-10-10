using UnityEngine;

public class ConsumableItem : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public PlayerStats playerStats;

    public void UseItem(Item item)
    {
        if (item == null || item.data == null) return;
        if (item.data.type != ItemType.Consumable) return;

        // ���������, �������� �� ������� ����
        FoodData foodData = item.data as FoodData;

        if (foodData != null)
        {
            // ��������� ������ ���
            playerStats.Eat(foodData.hungerRestored);
            playerStats.Rest(foodData.energyRestored);
        }
        else
        {
            // ���� ��� ������� Consumable ��� FoodData
            playerStats.Eat(item.data.value);
        }

        // ������� ������� �� ���������
        playerInventory.inventory.Remove(item);

        // ��������� UI ���������
        if (playerInventory.inventoryUI != null)
            playerInventory.inventoryUI.UpdateInventory();

        Debug.Log($"������� {item.data.itemName} �����������!");
    }
}
