using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("�������� ����������")]
    public int maxHunger = 100;
    public int currentHunger;

    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("������� ����������")]
    public float hungerDecayRate = 1f;  // ������ X ������
    public float energyDecayRate = 1.5f; // ������ X ������
    public int hungerLossPerTick = 1;
    public int energyLossPerTick = 1;

    [Header("�����")]
    public PlayerHealth playerHealth;

    [Header("UI")]
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI energyText;

    private float hungerTimer;
    private float energyTimer;

    private bool isNearCampfire = false;
    private bool isNight = false;
    void Start()
    {
        // ������������� �����������
        currentHunger = maxHunger;
        currentEnergy = maxEnergy;

        // ����� � PlayerHealth
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        UpdateUI();
    }

    void Update()
    {
        HandleHunger();
        HandleEnergy();
        CheckCriticalStates();

        if (isNight && !isNearCampfire)
        {
            // �������� ������� ����������� �����
            hungerDecayRate = 0.5f;
            energyDecayRate = 0.75f;
        }
        else
        {
            hungerDecayRate = 1f;
            energyDecayRate = 1.5f;
        }
    }

    void HandleHunger()
    {
        hungerTimer += Time.deltaTime;
        if (hungerTimer >= hungerDecayRate)
        {
            hungerTimer = 0f;
            currentHunger = Mathf.Max(0, currentHunger - hungerLossPerTick);
            UpdateUI();
        }
    }

    void HandleEnergy()
    {
        energyTimer += Time.deltaTime;
        if (energyTimer >= energyDecayRate)
        {
            energyTimer = 0f;
            currentEnergy = Mathf.Max(0, currentEnergy - energyLossPerTick);
            UpdateUI();
        }
    }

    void CheckCriticalStates()
    {
        // ����� �������� ���� ������ ���� �����/������� ����� 0
        if (currentHunger <= 0 && playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }

        if (currentEnergy <= 0 && playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
    }

    public void Eat(int amount)
    {
        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
        UpdateUI();
        Debug.Log($"����� ����, ����� ������������ �� {amount}");
    }

    public void Rest(int amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        UpdateUI();
        Debug.Log($"����� ����������� ������� �� {amount}");
    }

    public void UpdateUI()
    {
        if (hungerText != null)
            hungerText.text = $"�����: {currentHunger}/{maxHunger}";
        if (energyText != null)
            energyText.text = $"�������: {currentEnergy}/{maxEnergy}";
    }

    void OnEnable()
    {
        DayNightCycle.OnNightStart += () => isNight = true;
        DayNightCycle.OnDayStart += () => isNight = false;
    }

    void OnDisable()
    {
        DayNightCycle.OnNightStart -= () => isNight = true;
        DayNightCycle.OnDayStart -= () => isNight = false;
    }

    public void SetNearCampfire(bool state)
    {
        isNearCampfire = state;
    }
}
