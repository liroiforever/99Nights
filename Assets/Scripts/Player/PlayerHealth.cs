using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void UpdateUI()
    {
        if (healthText != null)
            healthText.SetText("��������: " + currentHealth);
    }


    void Die()
    {
        Debug.Log("����� �����!");
        // ����� �������� ������� ��� ������� �����
    }
}
