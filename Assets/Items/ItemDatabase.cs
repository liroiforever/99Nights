using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

[Header("Все предметы в игре")]
    public List<ItemData> allItems = new List<ItemData>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Поиск ItemData по имени предмета (регистр чувствителен)
    /// </summary>
    public static ItemData GetItem(string name)
    {
        if (Instance == null)
        {
            Debug.LogError("ItemDatabase не найден в сцене!");
            return null;
        }

        return Instance.allItems.Find(i => i.itemName == name);
    }

}
