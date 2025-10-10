using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HotbarSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public HotbarUI hotbarUI;
    public int slotIndex;

    private GameObject draggedIcon;

    public void OnBeginDrag(PointerEventData eventData)
    {
        int invIndex = hotbarUI.playerInventory.quickSlotIndices[slotIndex];
        if (invIndex < 0 || invIndex >= hotbarUI.playerInventory.inventory.Count)
            return;

        Item item = hotbarUI.playerInventory.inventory[invIndex];
        if (item == null) return;

        // Создаём перетаскиваемую иконку
        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(hotbarUI.transform.root);
        draggedIcon.transform.SetAsLastSibling();

        Image img = draggedIcon.AddComponent<Image>();
        img.sprite = item.icon;
        img.raycastTarget = false;

        RectTransform slotRect = GetComponent<RectTransform>();
        RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
        dragRect.sizeDelta = slotRect.sizeDelta;

        CanvasGroup cg = draggedIcon.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        draggedIcon.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            draggedIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            Destroy(draggedIcon);
    }
}
