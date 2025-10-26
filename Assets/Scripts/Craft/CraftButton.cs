using System.Collections.Generic;
using UnityEngine;

public class CraftButton : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public ItemData craftedItem; // ScriptableObject � ������� ��������
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

        // �������� ��������
        foreach (var req in recipe)
        {
            if (!playerInventory.HasResource(req.resourceType, req.amount))
            {
                Debug.Log($"? �� ������� {req.resourceType} ��� {craftedItem.itemName}");
                return;
            }
        }

        // �������� ��������
        foreach (var req in recipe)
            playerInventory.ConsumeResource(req.resourceType, req.amount);

        // ��������� �������
        playerInventory.AddItem(craftedItem);
        Debug.Log($"? ������ �������: {craftedItem.itemName}");
    }

}
