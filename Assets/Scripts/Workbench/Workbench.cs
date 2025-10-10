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
    public GameObject craftingUI; // Canvas � �������� ������
    public PlayerInventory playerInventory;

    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
        if (craftingUI != null)
            craftingUI.SetActive(false);
    }

    // ����� ������� � ��������
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

    // ���� ����� ����� ��������� � ������� UI
    public void CraftItem(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Count) return;

        Recipe recipe = recipes[recipeIndex];

        // �������� ��������
        foreach (var req in recipe.requirements)
        {
            if (!playerInventory.resources.ContainsKey(req.resourceType) ||
                playerInventory.resources[req.resourceType] < req.amount)
            {
                Debug.Log($"? �� ������� {req.resourceType} ��� {recipe.result.itemName}");
                return;
            }
        }

        // ������ ��������
        foreach (var req in recipe.requirements)
            playerInventory.resources[req.resourceType] -= req.amount;

        // ���������� ��������
        playerInventory.AddItem(recipe.result);
        playerInventory.UpdateUI();
        Debug.Log($"? ������ �������: {recipe.result.itemName}");
    }
}
