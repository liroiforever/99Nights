using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Основные показатели")]
    public int maxHunger = 100;
    public int currentHunger;

    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Падение параметров")]
    public float hungerDecayRate = 1f;  // каждые X секунд
    public float energyDecayRate = 1.5f; // каждые X секунд
    public int hungerLossPerTick = 1;
    public int energyLossPerTick = 1;

    [Header("Связи")]
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
        // Инициализация показателей
        currentHunger = maxHunger;
        currentEnergy = maxEnergy;

        // Связь с PlayerHealth
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
            // ускоряем падение показателей ночью
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
        // Игрок получает урон только если голод/энергия равны 0
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
        Debug.Log($"Игрок поел, голод восстановлен на {amount}");
    }

    public void Rest(int amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        UpdateUI();
        Debug.Log($"Игрок восстановил энергию на {amount}");
    }

    public void UpdateUI()
    {
        if (hungerText != null)
            hungerText.text = $"Голод: {currentHunger}/{maxHunger}";
        if (energyText != null)
            energyText.text = $"Энергия: {currentEnergy}/{maxEnergy}";
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
