using System.Collections.Generic;
using UnityEngine;

public class CraftSystem : MonoBehaviour
{
    public List<CraftRecipeData> recipes = new List<CraftRecipeData>();
    public PlayerInventory playerInventory;

    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
    }

    // Метод крафта по индексу рецепта
    public void Craft(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Count) return;

        CraftRecipeData recipe = recipes[recipeIndex];

        // Проверка ресурсов
        foreach (var req in recipe.requirements)
        {
            if (!playerInventory.HasResource(req.resourceType, req.amount))
            {
                Debug.Log($"? Не хватает {req.resourceType} для {recipe.result.itemName}");
                return;
            }
        }

        // Списание ресурсов
        foreach (var req in recipe.requirements)
            playerInventory.ConsumeResource(req.resourceType, req.amount);

        // Добавляем предмет
        playerInventory.AddItem(recipe.result);
        playerInventory.UpdateUI();

        Debug.Log($"? Создан предмет: {recipe.result.itemName}");
    }

}
