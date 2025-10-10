using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;                 // ������ ��������
    public Item currentItem;           // ������� � ���� �����
    public InventoryUI inventoryUI;    // ������ �� ���������
    public TextMeshProUGUI amountText;
    public DragPlaceableItem placementManager;

    private GameObject draggedIcon;    // ��������� ������ ��� ��������������

    void Awake()
    {
        // ������������� ������� PlacementManager � �����
        if (placementManager == null)
            placementManager = FindObjectOfType<DragPlaceableItem>();
    }

    // ���������� �������
    public void SetItem(Item newItem)
    {
        currentItem = newItem;

        if (newItem != null && newItem.icon != null)
        {
            icon.sprite = newItem.icon;
            icon.enabled = true;
            amountText.text = newItem.amount > 1 ? newItem.amount.ToString() : "";
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
            amountText.text = "";
        }
    }

    // ������ ��������������
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Drag started: {currentItem?.data?.itemName ?? "null"}");

        if (currentItem == null) return;

        // ���� ������� ����� ��������� �� ��������
        if (placementManager != null && currentItem.data.placeablePrefab != null)
        {
            placementManager.BeginPlace(currentItem);
            return; // �� ������ ����������� draggedIcon UI
        }

        // ������� �������������� UI
        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(inventoryUI.transform.root);
        draggedIcon.transform.SetAsLastSibling();

        Image img = draggedIcon.AddComponent<Image>();
        img.sprite = currentItem.icon;
        img.raycastTarget = false;

        RectTransform slotRect = icon.GetComponent<RectTransform>();
        RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
        dragRect.sizeDelta = slotRect.sizeDelta;

        CanvasGroup cg = draggedIcon.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        draggedIcon.transform.position = eventData.position;
    }

    // ����������� ������
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            draggedIcon.transform.position = eventData.position;
    }

    // ���������� ��������������
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            Destroy(draggedIcon);
    }

    // ���������� �������� �� ���� ��� ������
    public void OnDrop(PointerEventData eventData)
    {
        // ������� ������� � ������
        HotbarSlotDrop hotbarDrop = eventData.pointerDrag?.GetComponent<HotbarSlotDrop>();
        if (hotbarDrop != null) return; // ������������ � HotbarSlotDrop

        // ������� ������� ������� �� ������� � ���������
        HotbarSlot hotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlot>();
        if (hotbarSlot != null)
        {
            int invIndex = hotbarSlot.hotbarUI.playerInventory.quickSlotIndices[hotbarSlot.slotIndex];
            if (invIndex >= 0 && invIndex < hotbarSlot.hotbarUI.playerInventory.inventory.Count)
            {
                Item item = hotbarSlot.hotbarUI.playerInventory.inventory[invIndex];

                // ������� �������
                hotbarSlot.hotbarUI.playerInventory.quickSlotIndices[hotbarSlot.slotIndex] = -1;

                // ��������� � ���������, ���� ��� �� ��������
                if (!inventoryUI.playerInventory.inventory.Contains(item))
                    inventoryUI.playerInventory.inventory.Add(item);

                hotbarSlot.hotbarUI.UpdateIcons();
                inventoryUI.UpdateInventory();

                Debug.Log($"������� {item.name} ��������� �� ������� � ������");
            }
        }
    }

    // ���������� ������ �����
    public void UpdateIcon(Item item)
    {
        SetItem(item);
    }
}
