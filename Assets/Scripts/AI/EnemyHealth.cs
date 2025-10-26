using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("��������� ��������")]
    public int maxHealth = 50;
    public int currentHealth;

    private EnemyAI enemyAI;

    // --- ��������� ---
    [Header("���� ������ � ���������")]
    public bool isBuilding = false;
    public BuildingDestructible buildingDestructible;
    // ------------------

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();

        // --- ��������� ---
        if (isBuilding && buildingDestructible == null)
            buildingDestructible = GetComponent<BuildingDestructible>();
        // ------------------
    }

    void OnEnable()
    {
        currentHealth = maxHealth;

        // --- ��������� ---
        if (isBuilding && buildingDestructible != null)
        {
            buildingDestructible.Repair(maxHealth);
            return; // � �������� �� ����� ������������ EnemyAI
        }
        // ------------------

        if (enemyAI != null)
            enemyAI.isActive = true;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // --- ��������� ---
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
            // � �������� � ���� ������ "������"
            if (buildingDestructible != null)
                buildingDestructible.TakeDamage(maxHealth); // ��������
            return;
        }

        Debug.Log($"{gameObject.name} ����!");

        if (enemyAI != null)
            enemyAI.isActive = false;

        // ������ ����������� � ���������, ����� ������������ ��������
        gameObject.SetActive(false);
    }
}
