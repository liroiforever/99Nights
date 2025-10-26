using System.Collections.Generic;
using UnityEngine;

public class CraftButton : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public ItemData craftedItem; // ScriptableObject с данными предмета
    public List<ResourceRequirement> recipe = new List<ResourceRequirement>();

    [System.Serializable]
    public class ResourceRequirement
    {
        public string resourceType;
        public int amount;
    }

    public void CraftItem()
    {
        if (playerInventory == null || craftedItem == null) return;

        // Проверка ресурсов
        foreach (var req in recipe)
        {
            if (!playerInventory.HasResource(req.resourceType, req.amount))
            {
                Debug.Log($"? Не хватает {req.resourceType} для {craftedItem.itemName}");
                return;
            }
        }

        // Списание ресурсов
        foreach (var req in recipe)
            playerInventory.ConsumeResource(req.resourceType, req.amount);

        // Добавляем предмет
        playerInventory.AddItem(craftedItem);
        Debug.Log($"? Создан предмет: {craftedItem.itemName}");
    }

}
