using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    public ItemData itemData;          // Ссылка на ItemData
    public Transform player;           // Игрок
    public float placeDistance = 2f;   // Расстояние спавна от игрока

    private PlayerInventory inventory;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        inventory = player.GetComponent<PlayerInventory>();
    }

    // Метод размещения предмета из инвентаря на террейне
    public void PlaceItem()
    {
        if (itemData == null || itemData.placeablePrefab == null || inventory == null) return;

        Item selectedItem = inventory.inventory.Find(i => i.data == itemData);
        if (selectedItem == null || selectedItem.amount <= 0)
        {
            Debug.Log("Нет предмета для размещения");
            return;
        }

        Vector3 spawnPos = player.position + player.forward * placeDistance;
        Instantiate(itemData.placeablePrefab, spawnPos, Quaternion.identity);

        selectedItem.amount--;
        if (selectedItem.amount <= 0)
            inventory.inventory.Remove(selectedItem);

        inventory.UpdateUI();
        Debug.Log($"Размещен {itemData.itemName} на сцене");
    }

}
