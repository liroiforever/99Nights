using UnityEngine;
using UnityEngine.EventSystems;

public class DragPlaceableItem : MonoBehaviour
{
    [Header("������")]
    public PlayerInventory inventory;       // ����� / ���������
    public Camera mainCamera;               // ������ ��� raycast
    public float placeDistance = 2f;        // ���������� �� ���������, ���� raycast �� �����
    public LayerMask groundLayer;           // ���� ��������/�����

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
    /// ���������� �� InventorySlot ��� ������ drag ��������
    /// </summary>
    public void BeginPlace(Item item)
    {
        if (item == null || item.data == null || item.data.placeablePrefab == null) return;

        currentItem = item;

        // ������� ghost-������ (����������)
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

        // �������� ��������� (������ ��� �������)
        if (Input.GetKey(KeyCode.R))
            ghostPrefab.transform.Rotate(Vector3.up, 100f * Time.deltaTime);

        // ��� � ����������
        if (Input.GetMouseButtonDown(0))
        {
            PlaceItem();
        }

        // ��� ��� ESC � ������
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    private void PlaceItem()
    {
        if (currentItem == null || ghostPrefab == null) return;

        // ������� �������� ������
        Instantiate(currentItem.data.placeablePrefab, ghostPrefab.transform.position, ghostPrefab.transform.rotation);

        // ����� ��������������� Item � inventory �� ItemData (��������, ��� ��������� ������)
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
            // �������� �������� � ��������� ����� (���� data �����������)
            if (it.data == null && currentItem.data == null && it.name == currentItem.name)
            {
                foundIndex = i;
                invItem = it;
                break;
            }
        }

        if (invItem == null)
        {
            // fallback � ��������� currentItem ���� �������� (�� ��� ����� ������)
            Debug.LogWarning("[DragPlaceableItem] �� ������ ���������� � inventory �� data � �������� fallback � currentItem.");
            currentItem.amount--;
            // ������� ������ �� ������ �� ������, ���� ��� ��� ����
            int idx = inventory.inventory.IndexOf(currentItem);
            if (currentItem.amount <= 0 && idx >= 0)
            {
                inventory.inventory.RemoveAt(idx);
                // ������� ������ �� ������� idx � �������� ������� �������
                ClearHotbarIndicesAfterRemoval(idx);
            }
        }
        else
        {
            // ��������� ��������� ����
            invItem.amount--;
            Debug.Log($"[DragPlaceableItem] Placed {invItem.data.itemName}. New stack: {invItem.amount}");

            if (invItem.amount <= 0)
            {
                // ������� �� inventory �� ���������� �������
                inventory.inventory.RemoveAt(foundIndex);
                // ������� ������/�������� �������
                ClearHotbarIndicesAfterRemoval(foundIndex);
            }
        }

        // ��������� UI
        // PlayerInventory.UpdateUI ��������� �������, �� ��� ���������/������� ����� ��������� ������
        inventory.UpdateUI();
        if (inventory.inventoryUI != null) inventory.inventoryUI.UpdateInventory();
        if (inventory.hotbarUI != null) inventory.hotbarUI.UpdateIcons();

        // ������� preview
        Destroy(ghostPrefab);
        ghostPrefab = null;
        currentItem = null;
        isPlacing = false;
    }

    // ������� ������-������ �� �������� ������ � �������� ������� > removedIndex
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
                // �������� ������ �� -1, ��� ��� ������ ����������
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
