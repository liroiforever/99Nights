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

    // ����� ������ �� ������� �������
    public void Craft(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Count) return;

        CraftRecipeData recipe = recipes[recipeIndex];

        // �������� ��������
        foreach (var req in recipe.requirements)
        {
            if (!playerInventory.HasResource(req.resourceType, req.amount))
            {
                Debug.Log($"? �� ������� {req.resourceType} ��� {recipe.result.itemName}");
                return;
            }
        }

        // �������� ��������
        foreach (var req in recipe.requirements)
            playerInventory.ConsumeResource(req.resourceType, req.amount);

        // ��������� �������
        playerInventory.AddItem(recipe.result);
        playerInventory.UpdateUI();

        Debug.Log($"? ������ �������: {recipe.result.itemName}");
    }

}
