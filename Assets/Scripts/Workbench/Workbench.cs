using System.Collections.Generic;
using UnityEngine;

public class Workbench : MonoBehaviour
{
    [System.Serializable]
    public class Recipe
    {
        public ItemData result;
        public List<RecipeRequirement> requirements = new List<RecipeRequirement>();
    }

    [System.Serializable]
    public class RecipeRequirement
    {
        public string resourceType;
        public int amount;
    }

    public List<Recipe> recipes = new List<Recipe>();
    public GameObject craftingUI; // Canvas с кнопками крафта
    public PlayerInventory playerInventory;

    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
        if (craftingUI != null)
            craftingUI.SetActive(false);
    }

    // Игрок подошел к верстаку
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && craftingUI != null)
            craftingUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && craftingUI != null)
            craftingUI.SetActive(false);
    }

    // Этот метод можно привязать к кнопкам UI
    public void CraftItem(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Count) return;

        Recipe recipe = recipes[recipeIndex];

        // Проверка ресурсов
        foreach (var req in recipe.requirements)
        {
            if (!playerInventory.resources.ContainsKey(req.resourceType) ||
                playerInventory.resources[req.resourceType] < req.amount)
            {
                Debug.Log($"? Не хватает {req.resourceType} для {recipe.result.itemName}");
                return;
            }
        }

        // Снятие ресурсов
        foreach (var req in recipe.requirements)
            playerInventory.resources[req.resourceType] -= req.amount;

        // Добавление предмета
        playerInventory.AddItem(recipe.result);
        playerInventory.UpdateUI();
        Debug.Log($"? Создан предмет: {recipe.result.itemName}");
    }
}
