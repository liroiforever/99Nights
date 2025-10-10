// PlayerHealth.cs (���������� ��� �������� � ��������)
using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("������� ��������")]
    public float respawnBlinkDuration = 2f;   // ������� ������ �������
    public int blinkCount = 6;                // ���������� �������
    public GameObject respawnEffectPrefab;   // ��������, ������� ��� glow

    private Renderer[] renderers;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
        renderers = GetComponentsInChildren<Renderer>();
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
        Respawn();
    }

    public void Respawn()
    {
        // ������� �� ����� ������
        if (SpawnPoint.Instance != null)
        {
            transform.position = SpawnPoint.Instance.transform.position;
            transform.rotation = SpawnPoint.Instance.transform.rotation;
        }

        // �������������� ��������
        currentHealth = maxHealth;
        UpdateUI();

        // �������������� �����
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.currentHunger = stats.maxHunger;
            stats.currentEnergy = stats.maxEnergy;
            stats.UpdateUI();
        }

        // ������ ��������
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        StartCoroutine(RespawnBlink());
        Debug.Log("����� ��������� �� SpawnPoint");
    }

    private IEnumerator RespawnBlink()
    {
        if (renderers == null || renderers.Length == 0) yield break;

        float blinkInterval = respawnBlinkDuration / blinkCount / 2f;

        for (int i = 0; i < blinkCount; i++)
        {
            SetRenderersEnabled(false);
            yield return new WaitForSeconds(blinkInterval);
            SetRenderersEnabled(true);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SetRenderersEnabled(bool state)
    {
        foreach (var r in renderers)
        {
            r.enabled = state;
        }
    }
}
