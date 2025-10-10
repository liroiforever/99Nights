using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("ѕараметры здоровь€")]
    public int maxHealth = 50;
    public int currentHealth;

    private EnemyAI enemyAI;

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        if (enemyAI != null)
            enemyAI.isActive = true;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} убит!");
        if (enemyAI != null)
            enemyAI.isActive = false;

        // вместо уничтожени€ Ч отключаем, чтобы использовать повторно
        gameObject.SetActive(false);
    }
}
