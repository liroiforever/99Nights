using UnityEngine;

public class CollectResource : MonoBehaviour
{
    public string resourceType = "Wood"; // Wood, Stone, Food
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.AddResource(resourceType, amount);
            }

            // Удаляем ресурс после сбора
            Destroy(gameObject);
        }
    }
}
