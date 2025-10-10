using UnityEngine;
using UnityEngine.EventSystems;

public class DragPlaceableItem : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerInventory inventory;       // Игрок / инвентарь
    public Camera mainCamera;               // Камера для raycast
    public float placeDistance = 2f;        // Расстояние по умолчанию, если raycast не попал
    public LayerMask groundLayer;           // Слой террейна/земли

    private Item currentItem;
    private GameObject ghostPrefab;
    private bool isPlacing = false;

    void Start()
    {
        if (inventory == null)
            inventory = FindObjectOfType<PlayerInventory>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    /// <summary>
    /// Вызывается из InventorySlot при начале drag предмета
    /// </summary>
    public void BeginPlace(Item item)
    {
        if (item == null || item.data == null || item.data.placeablePrefab == null) return;

        currentItem = item;

        // Создаем ghost-префаб (прозрачный)
        ghostPrefab = Instantiate(currentItem.data.placeablePrefab);
        SetGhostMaterial(ghostPrefab, 0.5f);
        isPlacing = true;

        Debug.Log($"[DragPlaceableItem] Begin placing: {currentItem.data.itemName} (stack {currentItem.amount})");
    }

    void Update()
    {
        if (!isPlacing || ghostPrefab == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            ghostPrefab.transform.position = hit.point;
            ghostPrefab.transform.rotation = Quaternion.identity;
        }
        else if (player != null)
        {
            ghostPrefab.transform.position = player.position + player.forward * placeDistance;
        }

        // Вращение постройки (удобно для заборов)
        if (Input.GetKey(KeyCode.R))
            ghostPrefab.transform.Rotate(Vector3.up, 100f * Time.deltaTime);

        // ЛКМ — установить
        if (Input.GetMouseButtonDown(0))
        {
            PlaceItem();
        }

        // ПКМ или ESC — отмена
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    private void PlaceItem()
    {
        if (currentItem == null || ghostPrefab == null) return;

        // Спавним реальный объект
        Instantiate(currentItem.data.placeablePrefab, ghostPrefab.transform.position, ghostPrefab.transform.rotation);

        // Найдём соответствующий Item в inventory по ItemData (надежнее, чем сравнение ссылок)
        int foundIndex = -1;
        Item invItem = null;
        for (int i = 0; i < inventory.inventory.Count; i++)
        {
            Item it = inventory.inventory[i];
            if (it == null) continue;
            if (it.data != null && currentItem.data != null && it.data == currentItem.data)
            {
                foundIndex = i;
                invItem = it;
                break;
            }
            // запасной критерий — сравнение имени (если data отсутствует)
            if (it.data == null && currentItem.data == null && it.name == currentItem.name)
            {
                foundIndex = i;
                invItem = it;
                break;
            }
        }

        if (invItem == null)
        {
            // fallback — уменьшаем currentItem если возможно (но это менее надёжно)
            Debug.LogWarning("[DragPlaceableItem] Не найден эквивалент в inventory по data — применяю fallback к currentItem.");
            currentItem.amount--;
            // Попытка убрать по ссылке из списка, если она там есть
            int idx = inventory.inventory.IndexOf(currentItem);
            if (currentItem.amount <= 0 && idx >= 0)
            {
                inventory.inventory.RemoveAt(idx);
                // очищаем хотбар по индексу idx и сдвигаем большие индексы
                ClearHotbarIndicesAfterRemoval(idx);
            }
        }
        else
        {
            // уменьшаем найденный стак
            invItem.amount--;
            Debug.Log($"[DragPlaceableItem] Placed {invItem.data.itemName}. New stack: {invItem.amount}");

            if (invItem.amount <= 0)
            {
                // удаляем из inventory по найденному индексу
                inventory.inventory.RemoveAt(foundIndex);
                // очищаем хотбар/сдвигаем индексы
                ClearHotbarIndicesAfterRemoval(foundIndex);
            }
        }

        // Обновляем UI
        // PlayerInventory.UpdateUI обновляет ресурсы, но для инвентаря/хотбара нужны отдельные вызовы
        inventory.UpdateUI();
        if (inventory.inventoryUI != null) inventory.inventoryUI.UpdateInventory();
        if (inventory.hotbarUI != null) inventory.hotbarUI.UpdateIcons();

        // Удаляем preview
        Destroy(ghostPrefab);
        ghostPrefab = null;
        currentItem = null;
        isPlacing = false;
    }

    // Удаляет хотбар-ссылки на удалённый индекс и сдвигает индексы > removedIndex
    private void ClearHotbarIndicesAfterRemoval(int removedIndex)
    {
        if (inventory == null || inventory.quickSlotIndices == null) return;

        for (int i = 0; i < inventory.quickSlotIndices.Length; i++)
        {
            if (inventory.quickSlotIndices[i] == removedIndex)
            {
                inventory.quickSlotIndices[i] = -1;
            }
            else if (inventory.quickSlotIndices[i] > removedIndex)
            {
                // сдвигаем индекс на -1, так как список сократился
                inventory.quickSlotIndices[i] = inventory.quickSlotIndices[i] - 1;
            }
        }
    }

    private void CancelPlacement()
    {
        if (ghostPrefab != null)
            Destroy(ghostPrefab);

        ghostPrefab = null;
        currentItem = null;
        isPlacing = false;
        Debug.Log("[DragPlaceableItem] Placement canceled.");
    }

    private void SetGhostMaterial(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (!mat.HasProperty("_Color")) continue;

                Color c = mat.color;
                c.a = alpha;
                mat.color = c;

                mat.SetFloat("_Mode", 2); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }

    private Transform player
    {
        get
        {
            if (inventory != null)
                return inventory.transform;
            return null;
        }
    }
}
