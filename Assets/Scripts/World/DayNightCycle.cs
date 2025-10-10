using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [Header("��������� �����")]
    public float fullDayLength = 600f; // 10 ����� = 1 ������ ����
    [Range(0, 1)] public float currentTime = 0f; // 0 = �������, 0.5 = ����
    public Light sunLight;
    public Gradient lightColor;
    public AnimationCurve lightIntensity;

    [Header("���� � ����")]
    public bool isNight = false;
    public float nightStart = 0.45f;
    public float nightEnd = 0.75f;

    [Header("UI")]
    public TextMeshProUGUI nightCounterText;

    private int nightsSurvived = 0;

    public delegate void CycleEvent();
    public static event CycleEvent OnNightStart;
    public static event CycleEvent OnDayStart;

    void Update()
    {
        // ��� �����
        currentTime += Time.deltaTime / fullDayLength;
        if (currentTime >= 1f) currentTime = 0f;

        // �������� ����
        if (sunLight != null)
        {
            sunLight.color = lightColor.Evaluate(currentTime);
            sunLight.intensity = lightIntensity.Evaluate(currentTime);
            sunLight.transform.rotation = Quaternion.Euler(new Vector3((currentTime * 360f) - 90f, 170f, 0));
        }

        // ��������� ������� ����/����
        bool nowNight = (currentTime >= nightStart && currentTime < nightEnd);
        if (nowNight && !isNight)
        {
            isNight = true;
            OnNightStart?.Invoke();
            Debug.Log("��������� ����");
        }
        else if (!nowNight && isNight)
        {
            isNight = false;
            nightsSurvived++;
            OnDayStart?.Invoke();
            Debug.Log("�������� ���� � ����� �����: " + nightsSurvived);
            if (nightCounterText != null)
                nightCounterText.text = $"����� ��������: {nightsSurvived}";
        }
    }
}
