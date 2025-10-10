// PlayerHealth.cs (дополнение для респавна с эффектом)
using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Эффекты респавна")]
    public float respawnBlinkDuration = 2f;   // сколько секунд мерцать
    public int blinkCount = 6;                // количество миганий
    public GameObject respawnEffectPrefab;   // например, частицы или glow

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
            healthText.SetText("Здоровье: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("Игрок погиб!");
        Respawn();
    }

    public void Respawn()
    {
        // Перенос на точку спавна
        if (SpawnPoint.Instance != null)
        {
            transform.position = SpawnPoint.Instance.transform.position;
            transform.rotation = SpawnPoint.Instance.transform.rotation;
        }

        // Восстановление здоровья
        currentHealth = maxHealth;
        UpdateUI();

        // Восстановление статы
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.currentHunger = stats.maxHunger;
            stats.currentEnergy = stats.maxEnergy;
            stats.UpdateUI();
        }

        // Эффект респавна
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        StartCoroutine(RespawnBlink());
        Debug.Log("Игрок возрожден на SpawnPoint");
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
