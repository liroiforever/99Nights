using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    public ItemData itemData;          // ������ �� ItemData
    public Transform player;           // �����
    public float placeDistance = 2f;   // ���������� ������ �� ������

    private PlayerInventory inventory;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        inventory = player.GetComponent<PlayerInventory>();
    }

    // ����� ���������� �������� �� ��������� �� ��������
    public void PlaceItem()
    {
        if (itemData == null || itemData.placeablePrefab == null || inventory == null) return;

        Item selectedItem = inventory.inventory.Find(i => i.data == itemData);
        if (selectedItem == null || selectedItem.amount <= 0)
        {
            Debug.Log("��� �������� ��� ����������");
            return;
        }

        Vector3 spawnPos = player.position + player.forward * placeDistance;
        Instantiate(itemData.placeablePrefab, spawnPos, Quaternion.identity);

        selectedItem.amount--;
        if (selectedItem.amount <= 0)
            inventory.inventory.Remove(selectedItem);

        inventory.UpdateUI();
        Debug.Log($"�������� {itemData.itemName} �� �����");
    }

}
