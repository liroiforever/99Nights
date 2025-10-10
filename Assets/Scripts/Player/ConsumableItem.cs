using UnityEngine;

public class ConsumableItem : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public PlayerStats playerStats;

    public void UseItem(Item item)
    {
        if (item == null || item.data == null) return;
        if (item.data.type != ItemType.Consumable) return;

        // Проверяем, является ли предмет едой
        FoodData foodData = item.data as FoodData;

        if (foodData != null)
        {
            // Применяем эффект еды
            playerStats.Eat(foodData.hungerRestored);
            playerStats.Rest(foodData.energyRestored);
        }
        else
        {
            // Если это обычный Consumable без FoodData
            playerStats.Eat(item.data.value);
        }

        // Удаляем предмет из инвентаря
        playerInventory.inventory.Remove(item);

        // Обновляем UI инвентаря
        if (playerInventory.inventoryUI != null)
            playerInventory.inventoryUI.UpdateInventory();

        Debug.Log($"Предмет {item.data.itemName} использован!");
    }
}
