using UnityEngine;
using UnityEngine.UI;

public class Campfire : MonoBehaviour
{
    [Header("Настройки восстановления")]
    public int healthPerSecond = 5;
    public int energyPerSecond = 10;
    public float tickRate = 1f;

    [Header("Параметры костра")]
    public float maxBurnTime = 60f; // сколько секунд горит костер
    private float currentBurnTime;

    [Header("UI костра")]
    public Slider burnSlider; // ссылка на слайдер
    public GameObject fireEffect; // particle system (огонь)
    public Light fireLight; // Point Light для костра

    [Header("Подкидывание дров")]
    public KeyCode addFuelKey = KeyCode.E;  // клавиша подкидывания
    public float fuelAddAmount = 20f;       // сколько секунд добавляет одно полено
    public float interactionRadius = 3f;    // радиус взаимодействия

    private Transform player;
    private PlayerInventory playerInventory;
    private float timer = 0f;
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

        // Изначально включаем свет, если костёр активен
        if (fireLight != null)
            fireLight.intensity = currentBurnTime > 0 ? 5f : 0f;
    }

    void Update()
    {
        if (!isActive) return;

        // Горение костра
        currentBurnTime -= Time.deltaTime;
        if (burnSlider != null)
            burnSlider.value = currentBurnTime;

        // Плавная интенсивность Point Light
        if (fireLight != null)
        {
            fireLight.intensity = Mathf.Lerp(0f, 5f, Mathf.Clamp01(currentBurnTime / maxBurnTime));
        }

        // Если костёр потух
        if (currentBurnTime <= 0)
        {
            Extinguish();
        }

        // Проверяем взаимодействие с игроком
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
        if (playerInventory == null)
        {
            Debug.LogWarning("Нет ссылки на PlayerInventory!");
            return;
        }

        // Проверяем наличие дерева
        if (playerInventory.resources.ContainsKey("Wood") && playerInventory.resources["Wood"] > 0)
        {
            playerInventory.resources["Wood"] -= 1; // -1 дерево
            playerInventory.UpdateUI(); // обновляем UI
            AddFuel(fuelAddAmount);
            Debug.Log("Костер: добавлено 1 дерево, +20 секунд горения.");
        }
        else
        {
            Debug.Log("Нет дров для костра!");
        }
    }

    void Extinguish()
    {
        isActive = false;
        if (fireEffect != null)
            fireEffect.SetActive(false);
        if (fireLight != null)
            fireLight.intensity = 0f;

        Debug.Log("Костер потух!");
    }

    public void AddFuel(float amount)
    {
        currentBurnTime += amount;
        if (currentBurnTime > maxBurnTime)
            currentBurnTime = maxBurnTime;

        if (burnSlider != null)
            burnSlider.value = currentBurnTime;

        // Если костёр был потухший — разжигаем его заново
        if (!isActive)
        {
            isActive = true;
            if (fireEffect != null)
                fireEffect.SetActive(true);
            if (fireLight != null)
                fireLight.intensity = Mathf.Lerp(0f, 5f, Mathf.Clamp01(currentBurnTime / maxBurnTime));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
