using UnityEngine;
using UnityEngine.UI;
using TMPro; // для текста уровня

public class Campfire : MonoBehaviour
{
    [Header("Настройки восстановления")]
    public int healthPerSecond = 5;
    public int energyPerSecond = 10;
    public float tickRate = 1f;

    [Header("Параметры костра")]
    public float maxBurnTime = 60f;
    private float currentBurnTime;

    [Header("UI костра")]
    public Slider burnSlider;
    public GameObject fireEffect;
    public Light fireLight;

    [Header("Подкидывание дров")]
    public KeyCode addFuelKey = KeyCode.E;
    public float fuelAddAmount = 20f;
    public float interactionRadius = 3f;

    [Header("Туман / Fog of War")]
    public FogOfWarZone fogZone;
    public float fogIncreasePerLevel = 5f; // запасной вариант, если массив не заполнен
    public float[] fogExpandPerLevel; // прибавка радиуса для каждого уровня

    [Header("Уровень костра")]
    public int campfireLevel = 0;
    public int maxLevel = 5;
    public int[] woodRequiredPerLevel = { 10, 20, 30, 50, 80 };
    private int currentWoodCount = 0;

    [Header("UI уровня")]
    public Slider levelSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelProgressText;

    private Transform player;
    private PlayerInventory playerInventory;
    private float timer;
    private bool isActive = true;

    void Start()
    {
        currentBurnTime = maxBurnTime;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerInventory = playerObj.GetComponent<PlayerInventory>();
        }

        if (burnSlider != null)
        {
            burnSlider.maxValue = maxBurnTime;
            burnSlider.value = currentBurnTime;
        }

        UpdateLevelUI();

        if (fireLight != null) fireLight.intensity = 20f;
    }

    void Update()
    {
        if (!isActive) return;

        currentBurnTime -= Time.deltaTime;
        if (burnSlider != null)
            burnSlider.value = currentBurnTime;

        if (fireLight != null)
            fireLight.intensity = Mathf.Lerp(0f, 5f, Mathf.Clamp01(currentBurnTime / maxBurnTime));

        if (currentBurnTime <= 0)
            Extinguish();

        if (player != null && Vector3.Distance(player.position, transform.position) <= interactionRadius)
        {
            if (Input.GetKeyDown(addFuelKey))
            {
                TryAddFuel();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        if (playerHealth != null && playerStats != null)
        {
            playerStats.SetNearCampfire(true);

            timer += Time.deltaTime;
            if (timer >= tickRate)
            {
                playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + healthPerSecond, playerHealth.maxHealth);
                playerStats.Rest(energyPerSecond);
                playerHealth.UpdateUI();
                playerStats.UpdateUI();

                timer = 0f;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.SetNearCampfire(false);
    }

    void TryAddFuel()
    {
        if (playerInventory == null) return;

        if (playerInventory.ConsumeResource("Wood", 1))
        {
            AddFuel(fuelAddAmount);

            if (campfireLevel < maxLevel)
            {
                currentWoodCount++;
                CheckLevelUp();
                UpdateLevelUI();
            }
        }
    }

    void CheckLevelUp()
    {
        if (campfireLevel >= maxLevel) return;

        if (currentWoodCount >= woodRequiredPerLevel[campfireLevel])
        {
            campfireLevel++;
            currentWoodCount = 0;

            // Новый радиус тумана: сумма всех приростов до текущего уровня
            if (fogZone != null)
            {
                float totalRadius = 0f;
                for (int i = 0; i < campfireLevel; i++)
                {
                    totalRadius += GetFogAddForLevel(i + 1);
                }
                fogZone.ExpandFog(totalRadius);
            }

            Debug.Log("Костёр достиг уровня " + campfireLevel);
        }
    }

    float GetFogAddForLevel(int level)
    {
        int idx = level - 1;
        if (fogExpandPerLevel != null && idx >= 0 && idx < fogExpandPerLevel.Length)
        {
            return fogExpandPerLevel[idx];
        }
        return fogIncreasePerLevel;
    }

    void UpdateLevelUI()
    {
        if (levelSlider != null)
        {
            if (campfireLevel < maxLevel)
                levelSlider.value = (float)currentWoodCount / woodRequiredPerLevel[campfireLevel];
            else
                levelSlider.value = 1f;
        }

        if (levelText != null)
            levelText.text = $"{campfireLevel}";

        if (levelProgressText != null)
        {
            if (campfireLevel < maxLevel)
                levelProgressText.text = $"{currentWoodCount} / {woodRequiredPerLevel[campfireLevel]}";
            else
                levelProgressText.text = "MAX";
        }
    }

    void Extinguish()
    {
        isActive = false;
        if (fireEffect) fireEffect.SetActive(false);
        if (fireLight) fireLight.intensity = 0f;
    }

    public void AddFuel(float amount)
    {
        currentBurnTime += amount;
        if (currentBurnTime > maxBurnTime)
            currentBurnTime = maxBurnTime;

        if (!isActive)
        {
            isActive = true;
            if (fireEffect) fireEffect.SetActive(true);
        }
    }

    public void UpgradeCampfireExternal()
    {
        if (campfireLevel < maxLevel)
        {
            campfireLevel++;
            currentWoodCount = 0;
            UpdateLevelUI();

            if (fogZone != null)
            {
                float totalRadius = 0f;
                for (int i = 0; i < campfireLevel; i++)
                {
                    totalRadius += GetFogAddForLevel(i + 1);
                }
                fogZone.ExpandFog(totalRadius);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
