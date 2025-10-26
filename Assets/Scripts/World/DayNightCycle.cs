using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("Настройки цикла")]
    public float fullDayLength = 600f; // 10 минут = полный день
    [Range(0, 1)] public float currentTime = 0f;
    public Light sunLight;
    public Gradient lightColor;
    public AnimationCurve lightIntensity;

    [Header("Фазы дня (равные доли)")]
    [Tooltip("Утро: 0.00–0.25, День: 0.25–0.50, Вечер: 0.50–0.75, Ночь: 0.75–1.00")]
    public float morningEnd = 0.25f;
    public float dayEnd = 0.50f;
    public float eveningEnd = 0.75f;

    [Header("UI")]
    public TextMeshProUGUI nightCounterText;

    private int nightsSurvived = 0;

    public delegate void CycleEvent();
    public static event CycleEvent OnNightStart;
    public static event CycleEvent OnDayStart;

    public enum TimeOfDay { Morning, Day, Evening, Night }
    public TimeOfDay currentPhase = TimeOfDay.Morning;

    public bool isNight => currentPhase == TimeOfDay.Night;

    [Header("Аудио для времени суток")]
    public AudioSource audioSource;
    public AudioClip morningClip;
    public AudioClip dayClip;
    public AudioClip eveningClip;
    public AudioClip nightClip;
    public float transitionSpeed = 1f; // Скорость плавного перехода громкости

    private AudioClip currentClip;
    private float targetVolume = 1f;

    void Update()
    {
        // Продвигаем время
        currentTime += Time.deltaTime / fullDayLength;
        if (currentTime >= 1f)
            currentTime = 0f;

        // Свет и положение солнца
        if (sunLight != null)
        {
            sunLight.color = lightColor.Evaluate(currentTime);
            sunLight.intensity = lightIntensity.Evaluate(currentTime);
            sunLight.transform.rotation = Quaternion.Euler(new Vector3((currentTime * 360f) - 90f, 0f, 0f));
        }

        // Определяем текущую фазу
        TimeOfDay newPhase = DeterminePhase(currentTime);
        if (newPhase != currentPhase)
        {
            HandlePhaseChange(newPhase);
        }

        // Плавная регулировка громкости
        if (audioSource != null)
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, Time.deltaTime * transitionSpeed);
    }

    TimeOfDay DeterminePhase(float time)
    {
        if (time < morningEnd) return TimeOfDay.Morning;
        if (time < dayEnd) return TimeOfDay.Day;
        if (time < eveningEnd) return TimeOfDay.Evening;
        return TimeOfDay.Night;
    }

    void HandlePhaseChange(TimeOfDay newPhase)
    {
        // Обработка смены дня/ночи
        if (currentPhase == TimeOfDay.Night && newPhase == TimeOfDay.Morning)
        {
            nightsSurvived++;
            OnDayStart?.Invoke();
            Debug.Log($"Наступил день — Выжил ночей: {nightsSurvived}");
            if (nightCounterText != null)
                nightCounterText.text = $"Ночей пережито: {nightsSurvived}";
        }
        else if (newPhase == TimeOfDay.Night && currentPhase != TimeOfDay.Night)
        {
            OnNightStart?.Invoke();
            Debug.Log("Наступила ночь");
        }

        // Переключаем аудиотрек
        PlayPhaseMusic(newPhase);
        currentPhase = newPhase;
    }

    void PlayPhaseMusic(TimeOfDay phase)
    {
        if (audioSource == null) return;

        AudioClip nextClip = null;
        switch (phase)
        {
            case TimeOfDay.Morning: nextClip = morningClip; break;
            case TimeOfDay.Day: nextClip = dayClip; break;
            case TimeOfDay.Evening: nextClip = eveningClip; break;
            case TimeOfDay.Night: nextClip = nightClip; break;
        }

        if (nextClip == null || nextClip == currentClip) return;

        // Плавная смена звука
        StartCoroutine(FadeToClip(nextClip));
    }

    System.Collections.IEnumerator FadeToClip(AudioClip newClip)
    {
        if (audioSource.isPlaying)
        {
            // Плавно уменьшаем громкость
            while (audioSource.volume > 0.01f)
            {
                audioSource.volume -= Time.deltaTime * transitionSpeed;
                yield return null;
            }
        }

        // Меняем трек
        audioSource.clip = newClip;
        audioSource.Play();
        currentClip = newClip;

        // Плавно возвращаем громкость
        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += Time.deltaTime * transitionSpeed;
            yield return null;
        }
    }
}
