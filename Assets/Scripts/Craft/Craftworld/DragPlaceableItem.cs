using UnityEngine;
using UnityEngine.EventSystems;

public class DragPlaceableItem : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerInventory inventory;       
    public Camera mainCamera;               
    public float placeDistance = 2f;        
    public LayerMask groundLayer;           

    [Header("Настройки магнитного размещения")]
    public bool enableGridSnap = true;   
    public float snapSize = 1f;           

    [Header("Вращение")]
    public float rotationStep = 90f;      

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

    public void BeginPlace(Item item)
    {
        if (item == null || item.data == null || item.data.placeablePrefab == null) return;

        currentItem = item;

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
        }
        else if (player != null)
        {
            ghostPrefab.transform.position = player.position + player.forward * placeDistance;
        }

        if (enableGridSnap)
        {
            Vector3 pos = ghostPrefab.transform.position;
            pos.x = Mathf.Round(pos.x / snapSize) * snapSize;
            pos.y = Mathf.Round(pos.y / snapSize) * snapSize;
            pos.z = Mathf.Round(pos.z / snapSize) * snapSize;
            ghostPrefab.transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            float currentY = ghostPrefab.transform.eulerAngles.y;

            if (Mathf.Approximately(Mathf.Repeat(currentY, 360f), 0f))
            {
                ghostPrefab.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
            else
            {
                ghostPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            PlaceItem();
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }


    private void PlaceItem()
    {
        if (currentItem == null || ghostPrefab == null) return;

        Instantiate(currentItem.data.placeablePrefab, ghostPrefab.transform.position, ghostPrefab.transform.rotation);

        currentItem.amount--;
        if (currentItem.amount <= 0)
        {
            inventory.inventory.Remove(currentItem);

            for (int i = 0; i < inventory.quickSlotIndices.Length; i++)
                if (inventory.quickSlotIndices[i] == inventory.inventory.IndexOf(currentItem))
                    inventory.quickSlotIndices[i] = -1;
        }

        inventory.UpdateUI();
        if (inventory.inventoryUI != null) inventory.inventoryUI.UpdateInventory();
        if (inventory.hotbarUI != null) inventory.hotbarUI.UpdateIcons();

        // Удаляем preview
        Destroy(ghostPrefab);
        ghostPrefab = null;
        currentItem = null;
        isPlacing = false;
    }


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
                inventory.quickSlotIndices[i]--;
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
