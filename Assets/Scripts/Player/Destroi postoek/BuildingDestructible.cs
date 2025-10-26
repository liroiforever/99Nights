using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BuildingDestructible : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    [SerializeField] int currentHealth;

    [Header("Sprites (states)")]
    public Sprite intactSprite;      // �������� (�������)
    public Sprite damagedSprite;     // ������� ����
    public Sprite destroyedSprite;   // ��������� �������� (������)

    [Header("Options")]
    public bool destroyGameObjectOnZero = false; // ���� true � Destroy(gameObject) ��� 0
    public GameObject ruinPrefab;  // �������������: ��������� ������ "�����" ������ ���������
    public bool disableCollidersOnDestroyed = true;
    public float fadeOutSeconds = 0.5f; // ���� ������ ������� ������������

    [Header("Shake on hit ??")]
    public bool enableShake = true;
    public float shakeDuration = 0.1f;   // ������������
    public float shakeMagnitude = 0.08f; // ����

    // ������
    SpriteRenderer sr;
    Collider col;
    Collider2D col2D;
    Vector3 originalPos;
    Coroutine shakeRoutine;

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth > 0 ? currentHealth : maxHealth, 1, maxHealth);
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();
        originalPos = transform.localPosition;

        if (intactSprite != null)
            sr.sprite = intactSprite;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateVisualState();

        // ?? ������ ��� ���������
        if (enableShake)
        {
            if (shakeRoutine != null)
                StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(Shake(shakeDuration, shakeMagnitude));
        }

        if (currentHealth <= 0)
            OnDestroyed();
    }

    void UpdateVisualState()
    {
        if (sr == null) return;

        float ratio = (float)currentHealth / maxHealth;

        if (ratio <= 0f)
        {
            if (destroyedSprite != null) sr.sprite = destroyedSprite;
        }
        else if (ratio <= 0.5f)
        {
            if (damagedSprite != null) sr.sprite = damagedSprite;
        }
        else
        {
            if (intactSprite != null) sr.sprite = intactSprite;
        }
    }

    void OnDestroyed()
    {
        if (disableCollidersOnDestroyed)
        {
            if (col != null) col.enabled = false;
            if (col2D != null) col2D.enabled = false;
        }

        if (ruinPrefab != null)
        {
            Instantiate(ruinPrefab, transform.position, transform.rotation, transform.parent);
            if (destroyGameObjectOnZero)
                Destroy(gameObject);
            else
                sr.enabled = false;
            return;
        }

        if (destroyGameObjectOnZero)
        {
            if (fadeOutSeconds > 0f)
                StartCoroutine(FadeAndDestroy());
            else
                Destroy(gameObject);
        }
    }

    IEnumerator FadeAndDestroy()
    {
        Color start = sr.color;
        float t = 0f;
        while (t < fadeOutSeconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeOutSeconds);
            sr.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Repair(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateVisualState();
        if (currentHealth > 0)
        {
            if (sr != null) sr.enabled = true;
            if (col != null) col.enabled = true;
            if (col2D != null) col2D.enabled = true;
        }
    }

    public int GetCurrentHealth() => currentHealth;

    // ?? ������ ������
    IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
