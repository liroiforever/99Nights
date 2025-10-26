using UnityEngine;

public class PlaceableItemManager : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerInventory inventory;   // Игрок
    public Transform player;           // Игрок для позиции спавна
    public float placeDistance = 2f;   // Расстояние от игрока до спавна

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (inventory == null && player != null)
            inventory = player.GetComponent<PlayerInventory>();
    }

    // Метод для размещения выбранного предмета
    public void PlaceSelectedItem()
    {
        if (inventory == null) return;

        Item selectedItem = inventory.GetSelectedItem();
        if (selectedItem == null)
        {
            Debug.Log("Ничего не выбрано для размещения");
            return;
        }

        if (selectedItem.data.placeablePrefab == null)
        {
            Debug.Log($"{selectedItem.itemName} нельзя разместить на террейне");
            return;
        }

        Vector3 spawnPos = player.position + player.forward * placeDistance;
        Instantiate(selectedItem.data.placeablePrefab, spawnPos, Quaternion.identity);

        selectedItem.amount--;
        if (selectedItem.amount <= 0)
        {
            inventory.inventory.Remove(selectedItem);
            for (int i = 0; i < inventory.quickSlotIndices.Length; i++)
                if (inventory.quickSlotIndices[i] == inventory.inventory.IndexOf(selectedItem))
                    inventory.quickSlotIndices[i] = -1;
        }

        inventory.UpdateUI();
        Debug.Log($"{selectedItem.itemName} размещен на террейне");
    }

}
