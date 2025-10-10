using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("��������� ��������")]
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
        Debug.Log($"{gameObject.name} ����!");
        if (enemyAI != null)
            enemyAI.isActive = false;

        // ������ ����������� � ���������, ����� ������������ ��������
        gameObject.SetActive(false);
    }
}
