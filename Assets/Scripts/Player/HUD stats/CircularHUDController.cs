using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularHUDController : MonoBehaviour
{
    [Header("������ �� ������ (���������)")]
    public PlayerHealth playerHealth;
    public PlayerStats playerStats;
    public DayNightCycle dayNightCycle;

    [Header("�������� ���������� (�������� �� Z)")]
    public RectTransform hpCircle;
    public RectTransform energyCircle;
    public RectTransform hungerCircle;
    public RectTransform dayNightCircle;

    [Header("���� ����������� ��� ��������")]
    public Image hpImage;
    public Image energyImage;
    public Image hungerImage;

    [Header("������ (�����������)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI dayNightText;

    [Header("��������� ��������")]
    [Range(0, 360)] public float maxRotation = 360f;
    public bool invertRotation = true;
    public float rotationSmoothness = 5f;

    [Header("�������� ��� ������ ������")]
    public float lowStatThreshold = 0.25f; // ���� 25% ������ ������
    public float blinkSpeed = 4f;          // �������� �������
    public Color normalColor = Color.white;
    public Color blinkColor = Color.red;

    private float hpBlinkTimer;
    private float energyBlinkTimer;
    private float hungerBlinkTimer;

    void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();
    }

    void Update()
    {
        if (playerHealth == null || playerStats == null)
            return;

        UpdatePlayerCircles();
        UpdateDayNightCircle();
    }

    void UpdatePlayerCircles()
    {
        // --- �������� ---
        float hpPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        RotateSmooth(hpCircle, hpPercent);
        UpdateBlink(hpImage, hpPercent, ref hpBlinkTimer);

        if (hpText != null)
            hpText.text = $"{playerHealth.currentHealth}/{playerHealth.maxHealth}";

        // --- ������� ---
        float energyPercent = (float)playerStats.currentEnergy / playerStats.maxEnergy;
        RotateSmooth(energyCircle, energyPercent);
        UpdateBlink(energyImage, energyPercent, ref energyBlinkTimer);

        if (energyText != null)
            energyText.text = $"{playerStats.currentEnergy}/{playerStats.maxEnergy}";

        // --- ����� ---
        float hungerPercent = (float)playerStats.currentHunger / playerStats.maxHunger;
        RotateSmooth(hungerCircle, hungerPercent);
        UpdateBlink(hungerImage, hungerPercent, ref hungerBlinkTimer);

        if (hungerText != null)
            hungerText.text = $"{playerStats.currentHunger}/{playerStats.maxHunger}";
    }

    void UpdateDayNightCircle()
    {
        if (dayNightCycle == null || dayNightCircle == null)
            return;

        // �������� ���� ������� �� 0 �� 1
        float timePercent = Mathf.Clamp01(dayNightCycle.currentTime);

        // ����������� � � ���� � 0..360 ��������
        float angle = timePercent * 360f;

        // ��������� ����������� ��������
        if (invertRotation)
            angle = -angle;

        // ������� ������� ����� (�� ��� Z)
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        dayNightCircle.localRotation = Quaternion.Lerp(
            dayNightCircle.localRotation,
            targetRotation,
            Time.deltaTime * rotationSmoothness
        );

        // ��������� �����
        if (dayNightText != null)
            dayNightText.text = dayNightCycle.isNight ? "����" : "����";
    }


    void RotateSmooth(RectTransform circle, float percent)
    {
        if (!circle) return;
        float angle = (invertRotation ? -1 : 1) * percent * maxRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        circle.localRotation = Quaternion.Lerp(circle.localRotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }

    void UpdateBlink(Image image, float percent, ref float timer)
    {
        if (image == null) return;

        if (percent <= lowStatThreshold)
        {
            timer += Time.deltaTime * blinkSpeed;
            float t = (Mathf.Sin(timer) + 1f) / 2f; // ��������� �� 0 �� 1
            image.color = Color.Lerp(normalColor, blinkColor, t);
        }
        else
        {
            image.color = Color.Lerp(image.color, normalColor, Time.deltaTime * 5f);
        }
    }
}
