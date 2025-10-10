using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Настройки цикла")]
    public float fullDayLength = 600f; // 10 минут = полный день
    [Range(0, 1)] public float currentTime = 0f; // 0 = рассвет, 0.5 = ночь
    public Light sunLight;
    public Gradient lightColor;
    public AnimationCurve lightIntensity;

    [Header("Ночь и день")]
    public bool isNight = false;
    public float nightStart = 0.50f;
    public float nightEnd = 0.75f;

    [Header("UI")]
    public TextMeshProUGUI nightCounterText;

    private int nightsSurvived = 0;

    public delegate void CycleEvent();
    public static event CycleEvent OnNightStart;
    public static event CycleEvent OnDayStart;

    void Update()
    {
        // Продвигаем время
        currentTime += Time.deltaTime / fullDayLength;
        if (currentTime >= 1f) currentTime = 0f;

        // Настройка света
        if (sunLight != null)
        {
            sunLight.color = lightColor.Evaluate(currentTime);
            sunLight.intensity = lightIntensity.Evaluate(currentTime);

            // Солнце движется по небу: -90° -> 270° по X
            sunLight.transform.rotation = Quaternion.Euler(new Vector3((currentTime * 360f) - 90f, 0f, 0f));
        }

        // Проверка смены дня и ночи
        bool nowNight = (currentTime >= nightStart && currentTime < nightEnd);
        if (nowNight && !isNight)
        {
            isNight = true;
            OnNightStart?.Invoke();
            Debug.Log("Наступила ночь");
        }
        else if (!nowNight && isNight)
        {
            isNight = false;
            nightsSurvived++;
            OnDayStart?.Invoke();
            Debug.Log("Наступил день — Выжил ночей: " + nightsSurvived);
            if (nightCounterText != null)
                nightCounterText.text = $"Ночей пережито: {nightsSurvived}";
        }
    }
}
