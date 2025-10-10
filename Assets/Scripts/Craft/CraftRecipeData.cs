using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CraftRecipe", menuName = "Inventory/CraftRecipeData")]
public class CraftRecipeData : ScriptableObject
{
    public ItemData result; // предмет, который будет создан в инвентаре

    [System.Serializable]
    public class Requirement
    {
        public string resourceType; // Wood, Stone, Food
        public int amount;
    }

    public List<Requirement> requirements = new List<Requirement>();
    public float craftTime = 0f; // время крафта, 0 мгновенно
}
