using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Image[] slotImages;
    public TextMeshProUGUI[] slotAmountTexts; // текст для количества
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) playerInventory.selectedSlot = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) playerInventory.selectedSlot = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) playerInventory.selectedSlot = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) playerInventory.selectedSlot = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) playerInventory.selectedSlot = 4;

        UpdateSelection();

        if (Input.GetKeyDown(KeyCode.E))
            playerInventory.UseSelectedItem();
    }

    public void UpdateSelection()
    {
        for (int i = 0; i < slotImages.Length; i++)
            slotImages[i].color = (i == playerInventory.selectedSlot) ? selectedColor : normalColor;
    }

    public void UpdateIcons()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            int invIndex = playerInventory.quickSlotIndices[i];
            if (invIndex >= 0 && invIndex < playerInventory.inventory.Count)
            {
                Item item = playerInventory.inventory[invIndex];
                slotImages[i].sprite = item.icon;
                slotImages[i].color = Color.white;

                // Обновляем количество
                if (slotAmountTexts != null && i < slotAmountTexts.Length)
                    slotAmountTexts[i].text = item.amount > 1 ? item.amount.ToString() : "";
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1, 1, 1, 0);

                if (slotAmountTexts != null && i < slotAmountTexts.Length)
                    slotAmountTexts[i].text = "";
            }
        }

        UpdateSelection();
    }

    // Назначить предмет на слот хотбара
    public void AssignSlot(int slotIndex, int inventoryIndex)
    {
        playerInventory.quickSlotIndices[slotIndex] = inventoryIndex;
        UpdateIcons();
    }
}
