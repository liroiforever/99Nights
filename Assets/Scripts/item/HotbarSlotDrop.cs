using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotDrop : MonoBehaviour, IDropHandler
{
    public HotbarUI hotbar;
    public int slotIndex; // <-- привязываем вручную в инспекторе, от 0 до 4

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (draggedSlot == null || draggedSlot.currentItem == null)
            return;

        if (hotbar == null || hotbar.playerInventory == null)
        {
            Debug.LogWarning("Hotbar или playerInventory не привязаны!");
            return;
        }

        Item itemToMove = draggedSlot.currentItem;

        // Проверяем, есть ли уже этот предмет в других слотах хотбара
        for (int i = 0; i < hotbar.playerInventory.quickSlotIndices.Length; i++)
        {
            int invIndex = hotbar.playerInventory.quickSlotIndices[i];
            if (invIndex >= 0 && invIndex < hotbar.playerInventory.inventory.Count)
            {
                if (hotbar.playerInventory.inventory[invIndex] == itemToMove)
                {
                    Debug.Log("Предмет уже находится в хотбаре!");
                    return; // отменяем перетаскивание
                }
            }
        }

        // Получаем индекс предмета в основном inventory
        int indexInInventory = hotbar.playerInventory.inventory.IndexOf(itemToMove);
        if (indexInInventory < 0)
        {
            hotbar.playerInventory.inventory.Add(itemToMove);
            indexInInventory = hotbar.playerInventory.inventory.Count - 1;
        }

        // Назначаем слот хотбара на предмет
        hotbar.playerInventory.quickSlotIndices[slotIndex] = indexInInventory;

        // Очищаем слот рюкзака
        draggedSlot.SetItem(null);
        draggedSlot.UpdateIcon(null);

        // Обновляем UI
        draggedSlot.inventoryUI?.UpdateInventory();
        hotbar.UpdateIcons();

        Debug.Log($"Предмет {itemToMove.name} перенесён в хотбар слот {slotIndex + 1}");
    }
}
