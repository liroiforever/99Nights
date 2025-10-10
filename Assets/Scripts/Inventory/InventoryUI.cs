using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public GameObject slotPrefab;
    public Transform slotParent;

    private InventorySlot[] slots;

    void Awake()
    {
        if (slotPrefab != null && slotParent != null)
        {
            // Инициализируем, например, 20 слотов
            SetupSlots(20);
        }
        else
        {
            Debug.LogError("SlotPrefab или SlotParent не привязаны в InventoryUI!");
        }
    }

    public void UpdateInventory()
    {
        if (slots == null || slots.Length == 0) return;

        for (int i = 0; i < slots.Length; i++)
        {
            Item item = i < playerInventory.inventory.Count ? playerInventory.inventory[i] : null;
            slots[i].SetItem(item);
        }
    }


    // Метод для инициализации слотов
    public void SetupSlots(int size)
    {
        slots = new InventorySlot[size];
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            slots[i] = obj.GetComponent<InventorySlot>();
            slots[i].inventoryUI = this;
            slots[i].SetItem(null);
        }
    }
}
