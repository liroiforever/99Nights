using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotDrop : MonoBehaviour, IDropHandler
{
    public HotbarUI hotbar;
    public int slotIndex; // <-- ����������� ������� � ����������, �� 0 �� 4

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (draggedSlot == null || draggedSlot.currentItem == null)
            return;

        if (hotbar == null || hotbar.playerInventory == null)
        {
            Debug.LogWarning("Hotbar ��� playerInventory �� ���������!");
            return;
        }

        Item itemToMove = draggedSlot.currentItem;

        // ���������, ���� �� ��� ���� ������� � ������ ������ �������
        for (int i = 0; i < hotbar.playerInventory.quickSlotIndices.Length; i++)
        {
            int invIndex = hotbar.playerInventory.quickSlotIndices[i];
            if (invIndex >= 0 && invIndex < hotbar.playerInventory.inventory.Count)
            {
                if (hotbar.playerInventory.inventory[invIndex] == itemToMove)
                {
                    Debug.Log("������� ��� ��������� � �������!");
                    return; // �������� ��������������
                }
            }
        }

        // �������� ������ �������� � �������� inventory
        int indexInInventory = hotbar.playerInventory.inventory.IndexOf(itemToMove);
        if (indexInInventory < 0)
        {
            hotbar.playerInventory.inventory.Add(itemToMove);
            indexInInventory = hotbar.playerInventory.inventory.Count - 1;
        }

        // ��������� ���� ������� �� �������
        hotbar.playerInventory.quickSlotIndices[slotIndex] = indexInInventory;

        // ������� ���� �������
        draggedSlot.SetItem(null);
        draggedSlot.UpdateIcon(null);

        // ��������� UI
        draggedSlot.inventoryUI?.UpdateInventory();
        hotbar.UpdateIcons();

        Debug.Log($"������� {itemToMove.name} �������� � ������ ���� {slotIndex + 1}");
    }
}
