using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;                 // Иконка предмета
    public Item currentItem;           // Предмет в этом слоте
    public InventoryUI inventoryUI;    // Ссылка на инвентарь
    public TextMeshProUGUI amountText;
    public DragPlaceableItem placementManager;

    private GameObject draggedIcon;    // Временная иконка при перетаскивании

    void Awake()
    {
        // Автоматически находим PlacementManager в сцене
        if (placementManager == null)
            placementManager = FindObjectOfType<DragPlaceableItem>();
    }

    // Отображаем предмет
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

    // Начало перетаскивания
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Drag started: {currentItem?.data?.itemName ?? "null"}");

        if (currentItem == null) return;

        // Если предмет можно размещать на террейне
        if (placementManager != null && currentItem.data.placeablePrefab != null)
        {
            placementManager.BeginPlace(currentItem);
            return; // не создаём стандартный draggedIcon UI
        }

        // Обычное перетаскивание UI
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

    // Перемещение иконки
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            draggedIcon.transform.position = eventData.position;
    }

    // Завершение перетаскивания
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            Destroy(draggedIcon);
    }

    // Отпускание предмета на слот или хотбар
    public void OnDrop(PointerEventData eventData)
    {
        // Попытка бросить в хотбар
        HotbarSlotDrop hotbarDrop = eventData.pointerDrag?.GetComponent<HotbarSlotDrop>();
        if (hotbarDrop != null) return; // обработается в HotbarSlotDrop

        // Попытка вернуть предмет из хотбара в инвентарь
        HotbarSlot hotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlot>();
        if (hotbarSlot != null)
        {
            int invIndex = hotbarSlot.hotbarUI.playerInventory.quickSlotIndices[hotbarSlot.slotIndex];
            if (invIndex >= 0 && invIndex < hotbarSlot.hotbarUI.playerInventory.inventory.Count)
            {
                Item item = hotbarSlot.hotbarUI.playerInventory.inventory[invIndex];

                // Очистка хотбара
                hotbarSlot.hotbarUI.playerInventory.quickSlotIndices[hotbarSlot.slotIndex] = -1;

                // Добавляем в инвентарь, если ещё не добавлен
                if (!inventoryUI.playerInventory.inventory.Contains(item))
                    inventoryUI.playerInventory.inventory.Add(item);

                hotbarSlot.hotbarUI.UpdateIcons();
                inventoryUI.UpdateInventory();

                Debug.Log($"Предмет {item.name} возвращён из хотбара в рюкзак");
            }
        }
    }

    // Обновление иконки слота
    public void UpdateIcon(Item item)
    {
        SetItem(item);
    }
}
