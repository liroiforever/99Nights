using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Ресурсы")]
    public Dictionary<string, int> resources = new Dictionary<string, int>();
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI foodText;

    [Header("Инвентарь")]
    public List<Item> inventory = new List<Item>();     // основной список предметов
    public int[] quickSlotIndices = new int[5];         // слоты хотбара (ссылки на inventory)
    public int selectedSlot = 0;

    [Header("UI")]
    public HotbarUI hotbarUI;
    public InventoryUI inventoryUI;

    void Awake()
    {
        resources["Wood"] = 0;
        resources["Stone"] = 0;
        resources["Food"] = 0;
        UpdateUI();

        for (int i = 0; i < quickSlotIndices.Length; i++)
            quickSlotIndices[i] = -1;  // пустой слот

        if (hotbarUI == null)
            hotbarUI = FindObjectOfType<HotbarUI>();
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
    }

    public void AddResource(string type, int amount)
    {
        if (!resources.ContainsKey(type)) return;
        resources[type] += amount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (woodText != null) woodText.SetText(resources["Wood"].ToString());
        if (stoneText != null) stoneText.SetText(resources["Stone"].ToString());
        if (foodText != null) foodText.SetText(resources["Food"].ToString());
    }

    public void AddItem(ItemData itemData)
    {
        if (itemData == null) return;

        foreach (var itm in inventory)
        {
            if ((itm.data != null && itm.data == itemData) || itm.name == itemData.itemName)
            {
                if (itm.amount < itm.maxStack)
                {
                    itm.amount++;
                    inventoryUI.UpdateInventory();
                    hotbarUI.UpdateIcons();
                    Debug.Log($"Добавлен {itm.itemName}, теперь {itm.amount}");
                    return;
                }
            }
        }

        Item newItem = new Item
        {
            data = itemData,
            name = itemData.itemName,
            icon = itemData.icon,
            type = itemData.type,
            value = itemData.value,
            amount = 1,
            maxStack = itemData.maxStack
        };

        inventory.Add(newItem);
        inventoryUI.UpdateInventory();
        hotbarUI.UpdateIcons();
        Debug.Log($"Создан новый предмет: {newItem.itemName}");
    }

    public Item GetSelectedItem()
    {
        int index = quickSlotIndices[selectedSlot];
        if (index >= 0 && index < inventory.Count)
            return inventory[index];
        return null;
    }

    public void UseSelectedItem()
    {
        Item item = GetSelectedItem();
        if (item == null) return;

        switch (item.type)
        {
            case ItemType.Weapon:
                GetComponent<PlayerAttack>()?.UseWeapon(item.value);
                break;

            case ItemType.Consumable:
                // Проверяем, является ли этот Consumable едой
                if (item.data is FoodData food)
                {
                    PlayerStats stats = GetComponent<PlayerStats>();
                    if (stats != null)
                    {
                        stats.Eat(food.hungerRestored);
                        stats.Rest(food.energyRestored);
                    }
                }
                else
                {
                    // Если это не еда — лечим здоровье, как раньше
                    Heal(item.value);
                }

                // уменьшаем количество
                item.amount--;

                // обновляем UI
                inventoryUI.UpdateInventory();
                hotbarUI.UpdateIcons();

                // если предмет закончился — удаляем его
                if (item.amount <= 0)
                {
                    int removedIndex = inventory.IndexOf(item);
                    inventory.Remove(item);

                    // очищаем хотбар
                    for (int i = 0; i < quickSlotIndices.Length; i++)
                        if (quickSlotIndices[i] == removedIndex)
                            quickSlotIndices[i] = -1;

                    // обновляем UI снова
                    inventoryUI.UpdateInventory();
                    hotbarUI.UpdateIcons();
                }
                break;
        }
    }


    public void Heal(int amount)
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.currentHealth += amount;
            if (playerHealth.currentHealth > playerHealth.maxHealth)
                playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.UpdateUI();
        }
    }
}
