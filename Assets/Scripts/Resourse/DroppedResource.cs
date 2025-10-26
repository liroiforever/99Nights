using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DroppedResource : MonoBehaviour
{
    [Header("Resource Settings")]
    public string resourceType = "Wood";
    public int amount = 1;

    [Header("Lifetime Settings")]
    public float lifetime = 120f; // через 2 минуты сам исчезнет

    private void Start()
    {
        // гарантируем, что коллайдер — триггер
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;

        // автоудаление через lifetime секунд
        if (lifetime > 0)
            Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.AddResource(resourceType, amount);
                Debug.Log($"Подобран ресурс: {resourceType} +{amount}");
            }

            Destroy(gameObject);
        }
    }
}
