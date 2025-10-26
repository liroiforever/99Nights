using UnityEngine;

public class PlaceableItemManager : MonoBehaviour
{
    [Header("������")]
    public PlayerInventory inventory;   // �����
    public Transform player;           // ����� ��� ������� ������
    public float placeDistance = 2f;   // ���������� �� ������ �� ������

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (inventory == null && player != null)
            inventory = player.GetComponent<PlayerInventory>();
    }

    // ����� ��� ���������� ���������� ��������
    public void PlaceSelectedItem()
    {
        if (inventory == null) return;

        Item selectedItem = inventory.GetSelectedItem();
        if (selectedItem == null)
        {
            Debug.Log("������ �� ������� ��� ����������");
            return;
        }

        if (selectedItem.data.placeablePrefab == null)
        {
            Debug.Log($"{selectedItem.itemName} ������ ���������� �� ��������");
            return;
        }

        Vector3 spawnPos = player.position + player.forward * placeDistance;
        Instantiate(selectedItem.data.placeablePrefab, spawnPos, Quaternion.identity);

        selectedItem.amount--;
        if (selectedItem.amount <= 0)
        {
            inventory.inventory.Remove(selectedItem);
            for (int i = 0; i < inventory.quickSlotIndices.Length; i++)
                if (inventory.quickSlotIndices[i] == inventory.inventory.IndexOf(selectedItem))
                    inventory.quickSlotIndices[i] = -1;
        }

        inventory.UpdateUI();
        Debug.Log($"{selectedItem.itemName} �������� �� ��������");
    }

}
