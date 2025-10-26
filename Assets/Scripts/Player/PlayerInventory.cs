using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Инвентарь")]
    public List<Item> inventory = new List<Item>();
    public int[] quickSlotIndices = new int[5];
    public int selectedSlot = 0;

    [Header("UI")]
    public HotbarUI hotbarUI;
    public InventoryUI inventoryUI;

    void Awake()
    {
        // Инициализация хотбара
        for (int i = 0; i < quickSlotIndices.Length; i++)
            quickSlotIndices[i] = -1;

        if (hotbarUI == null)
            hotbarUI = FindObjectOfType<HotbarUI>();
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
    }

    // ======== Методы для работы с ресурсами ========
    public bool HasResource(string name, int amount)
    {
        Item item = inventory.Find(i => i.data != null && i.data.itemName == name);
        return item != null && item.amount >= amount;
    }

    public bool ConsumeResource(string name, int amount)
    {
        Item item = inventory.Find(i => i.data != null && i.data.itemName == name);
        if (item == null || item.amount < amount) return false;

        item.amount -= amount;
        if (item.amount <= 0)
            inventory.Remove(item);

        UpdateUI();
        return true;
    }

    public void AddResource(string name, int amount)
    {
        Item item = inventory.Find(i => i.data != null && i.data.itemName == name);
        if (item != null)
        {
            item.amount += amount;
        }
        else
        {
            // Берём ItemData из ItemDatabase
            ItemData data = ItemDatabase.GetItem(name);
            if (data != null)
            {
                AddItem(data, amount);
            }
            else
            {
                Debug.LogWarning($"Ресурс {name} не найден в ItemDatabase!");
                return;
            }
        }

        UpdateUI();
    }

    // ======== Добавление Item в inventory ========
    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null) return;

        foreach (var itm in inventory)
        {
            if (itm.data == itemData && itm.amount < itm.maxStack)
            {
                itm.amount += amount;
                inventoryUI?.UpdateInventory();
                hotbarUI?.UpdateIcons();
                return;
            }
        }

        Item newItem = new Item
        {
            data = itemData,
            name = itemData.itemName,
            icon = itemData.icon,
            type = itemData.type,
            value = itemData.value,
            amount = amount,
            maxStack = itemData.maxStack
        };

        inventory.Add(newItem);
        inventoryUI?.UpdateInventory();
        hotbarUI?.UpdateIcons();
    }

    // ======== Получение выбранного слота ========
    public Item GetSelectedItem()
    {
        int index = quickSlotIndices[selectedSlot];
        if (index >= 0 && index < inventory.Count)
            return inventory[index];
        return null;
    }

    // ======== Использование предмета ========
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
                if (item.data is FoodData food)
                {
                    PlayerStats stats = GetComponent<PlayerStats>();
                    stats?.Eat(food.hungerRestore);
                    stats?.Rest(food.energyRestore);

                    // если еда восстанавливает здоровье — используем существующий метод PlayerInventory.Heal
                    if (food.healthRestore > 0)
                        Heal(food.healthRestore);
                }
                else
                {
                    Heal(item.value);
                }

                item.amount--;
                if (item.amount <= 0)
                    inventory.Remove(item);

                UpdateUI();
                break;
        }
    }


    // ======== Восстановление здоровья ========
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

    // ======== Обновление UI ========
    public void UpdateUI()
    {
        inventoryUI?.UpdateInventory();
        hotbarUI?.UpdateIcons();
    }
}
