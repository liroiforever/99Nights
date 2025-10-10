using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularHUDController : MonoBehaviour
{
    [Header("Ссылки на игрока (автопоиск)")]
    public PlayerHealth playerHealth;
    public PlayerStats playerStats;
    public DayNightCycle dayNightCycle;

    [Header("Круговые индикаторы (вращение по Z)")]
    public RectTransform hpCircle;
    public RectTransform energyCircle;
    public RectTransform hungerCircle;
    public RectTransform dayNightCircle;

    [Header("Сами изображения для мерцания")]
    public Image hpImage;
    public Image energyImage;
    public Image hungerImage;

    [Header("Тексты (опционально)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI dayNightText;

    [Header("Настройки вращения")]
    [Range(0, 360)] public float maxRotation = 360f;
    public bool invertRotation = true;
    public float rotationSmoothness = 5f;

    [Header("Мерцание при низком уровне")]
    public float lowStatThreshold = 0.25f; // ниже 25% начнет мигать
    public float blinkSpeed = 4f;          // скорость мигания
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
        // --- Здоровье ---
        float hpPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        RotateSmooth(hpCircle, hpPercent);
        UpdateBlink(hpImage, hpPercent, ref hpBlinkTimer);

        if (hpText != null)
            hpText.text = $"{playerHealth.currentHealth}/{playerHealth.maxHealth}";

        // --- Энергия ---
        float energyPercent = (float)playerStats.currentEnergy / playerStats.maxEnergy;
        RotateSmooth(energyCircle, energyPercent);
        UpdateBlink(energyImage, energyPercent, ref energyBlinkTimer);

        if (energyText != null)
            energyText.text = $"{playerStats.currentEnergy}/{playerStats.maxEnergy}";

        // --- Голод ---
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

        // Получаем долю времени от 0 до 1
        float timePercent = Mathf.Clamp01(dayNightCycle.currentTime);

        // Преобразуем её в угол — 0..360 градусов
        float angle = timePercent * 360f;

        // Применяем направление вращения
        if (invertRotation)
            angle = -angle;

        // Плавный поворот круга (по оси Z)
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        dayNightCircle.localRotation = Quaternion.Lerp(
            dayNightCircle.localRotation,
            targetRotation,
            Time.deltaTime * rotationSmoothness
        );

        // Обновляем текст
        if (dayNightText != null)
            dayNightText.text = dayNightCycle.isNight ? "Ночь" : "День";
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
            float t = (Mathf.Sin(timer) + 1f) / 2f; // колебания от 0 до 1
            image.color = Color.Lerp(normalColor, blinkColor, t);
        }
        else
        {
            image.color = Color.Lerp(image.color, normalColor, Time.deltaTime * 5f);
        }
    }
}
