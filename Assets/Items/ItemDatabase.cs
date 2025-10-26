using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

[Header("��� �������� � ����")]
    public List<ItemData> allItems = new List<ItemData>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// ����� ItemData �� ����� �������� (������� ������������)
    /// </summary>
    public static ItemData GetItem(string name)
    {
        if (Instance == null)
        {
            Debug.LogError("ItemDatabase �� ������ � �����!");
            return null;
        }

        return Instance.allItems.Find(i => i.itemName == name);
    }

}
