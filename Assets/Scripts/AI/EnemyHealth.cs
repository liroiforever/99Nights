using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("ѕараметры здоровь€")]
    public int maxHealth = 50;
    public int currentHealth;

    private EnemyAI enemyAI;

    // --- ƒќЅј¬Ћ≈Ќќ ---
    [Header("≈сли объект Ч постройка")]
    public bool isBuilding = false;
    public BuildingDestructible buildingDestructible;
    // ------------------

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();

        // --- ƒќЅј¬Ћ≈Ќќ ---
        if (isBuilding && buildingDestructible == null)
            buildingDestructible = GetComponent<BuildingDestructible>();
        // ------------------
    }

    void OnEnable()
    {
        currentHealth = maxHealth;

        // --- ƒќЅј¬Ћ≈Ќќ ---
        if (isBuilding && buildingDestructible != null)
        {
            buildingDestructible.Repair(maxHealth);
            return; // у строений не нужно активировать EnemyAI
        }
        // ------------------

        if (enemyAI != null)
            enemyAI.isActive = true;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // --- ƒќЅј¬Ћ≈Ќќ ---
        if (isBuilding && buildingDestructible != null)
        {
            buildingDestructible.TakeDamage(amount);
        }
        // ------------------

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isBuilding)
        {
            // у строений Ч свой способ "смерти"
            if (buildingDestructible != null)
                buildingDestructible.TakeDamage(maxHealth); // добиваем
            return;
        }

        Debug.Log($"{gameObject.name} убит!");

        if (enemyAI != null)
            enemyAI.isActive = false;

        // вместо уничтожени€ Ч отключаем, чтобы использовать повторно
        gameObject.SetActive(false);
    }
}
