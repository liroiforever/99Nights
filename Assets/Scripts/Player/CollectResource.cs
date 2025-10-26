using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectResource : MonoBehaviour
{
    [Header("Resource Settings")]
    public string resourceType = "Wood"; // Wood, Stone, Food
    public int amount = 1;
    public GameObject dropPrefab; // Префаб дропа (опционально)

    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDepleted = false;

    [Header("Visual States (по стадиям повреждения)")]
    public Sprite intactSprite;   // 100%
    public Sprite damagedSprite;  // ~50%
    public Sprite heavilyDamagedSprite; // ~25%
    public Sprite depletedSprite; // 0% (обломок)
    private SpriteRenderer spriteRenderer;

    [Header("Shake Effect")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.15f;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip breakSound;
    public bool playSoundOnHit = true;
    private AudioSource audioSource;

    private Vector3 originalLocalPos;

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalLocalPos = transform.localPosition;

        // Проверяем наличие спрайта
        if (spriteRenderer != null && intactSprite != null)
            spriteRenderer.sprite = intactSprite;
    }

    void Update()
    {
        if (isDepleted) return;

        // Проверяем атаку игрока (через Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            PlayerAttack pAttack = player.GetComponent<PlayerAttack>();
            if (pAttack == null) return;

            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist <= pAttack.attackRange)
            {
                ApplyDamage(pAttack.attackDamage, player);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDepleted) return;

        if (other.CompareTag("Player"))
        {
            PlayerInventory inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
                inv.AddResource(resourceType, amount);

            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position, Quaternion.identity);

            SetDepletedState();
        }
    }

    public void ApplyDamage(int damage, GameObject playerWhoHit = null)
    {
        if (isDepleted || damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        StartCoroutine(ShakeCoroutine());

        if (playSoundOnHit && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        UpdateSpriteStage();

        if (currentHealth <= 0)
        {
            if (breakSound != null)
                audioSource.PlayOneShot(breakSound);

            if (playerWhoHit != null)
            {
                PlayerInventory inv = playerWhoHit.GetComponent<PlayerInventory>();
                if (inv != null)
                    inv.AddResource(resourceType, amount);
            }

            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position, Quaternion.identity);

            SetDepletedState();
        }
    }

    private void UpdateSpriteStage()
    {
        if (spriteRenderer == null) return;

        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent <= 0)
        {
            spriteRenderer.sprite = depletedSprite;
        }
        else if (healthPercent <= 0.25f && heavilyDamagedSprite != null)
        {
            spriteRenderer.sprite = heavilyDamagedSprite;
        }
        else if (healthPercent <= 0.5f && damagedSprite != null)
        {
            spriteRenderer.sprite = damagedSprite;
        }
        else if (intactSprite != null)
        {
            spriteRenderer.sprite = intactSprite;
        }
    }

    private void SetDepletedState()
    {
        isDepleted = true;
        currentHealth = 0;

        if (spriteRenderer != null && depletedSprite != null)
            spriteRenderer.sprite = depletedSprite;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false; // нельзя больше добывать

        StopAllCoroutines();
        transform.localPosition = originalLocalPos;
    }

    IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalLocalPos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (isDepleted) return;
        Vector3 pos = transform.position + Vector3.up * 1.5f;
        UnityEditor.Handles.Label(pos, $"HP: {currentHealth}/{maxHealth}");
    }
#endif
}
