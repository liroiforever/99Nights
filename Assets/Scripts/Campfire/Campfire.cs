using UnityEngine;
using UnityEngine.UI;

public class Campfire : MonoBehaviour
{
    [Header("Ќастройки восстановлени€")]
    public int healthPerSecond = 5;
    public int energyPerSecond = 10;
    public float tickRate = 1f;

    [Header("ѕараметры костра")]
    public float maxBurnTime = 60f; // сколько секунд горит костер
    private float currentBurnTime;

    [Header("UI костра")]
    public Slider burnSlider; // ссылка на слайдер
    public GameObject fireEffect; // particle system (огонь)

    [Header("ѕодкидывание дров")]
    public KeyCode addFuelKey = KeyCode.E;  // клавиша подкидывани€
    public float fuelAddAmount = 20f;       // сколько секунд добавл€ет одно полено
    public float interactionRadius = 3f;    // радиус взаимодействи€

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
    }

    void Update()
    {
        if (!isActive) return;

        // √орение костра
        currentBurnTime -= Time.deltaTime;
        if (burnSlider != null)
            burnSlider.value = currentBurnTime;

        if (currentBurnTime <= 0)
            Extinguish();

        // ѕровер€ем взаимодействие с игроком
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
            playerStats.SetNearCampfire(true); // <-- нова€ строка

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
            playerStats.SetNearCampfire(false); // <-- когда вышел из зоны
    }


    void TryAddFuel()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("Ќет ссылки на PlayerInventory!");
            return;
        }

        // ѕровер€ем наличие дерева
        if (playerInventory.resources.ContainsKey("Wood") && playerInventory.resources["Wood"] > 0)
        {
            playerInventory.resources["Wood"] -= 1; // -1 дерево
            playerInventory.UpdateUI(); // моментальное обновление UI
            AddFuel(fuelAddAmount);
            Debug.Log(" остер: добавлено 1 дерево, +20 секунд горени€.");
        }
        else
        {
            Debug.Log("Ќет дров дл€ костра!");
        }
    }

    void Extinguish()
    {
        isActive = false;
        if (fireEffect != null)
            fireEffect.SetActive(false);
        Debug.Log(" остер потух!");
    }

    public void AddFuel(float amount)
    {
        currentBurnTime += amount;
        if (currentBurnTime > maxBurnTime)
            currentBurnTime = maxBurnTime;

        if (burnSlider != null)
            burnSlider.value = currentBurnTime;

        if (!isActive)
        {
            isActive = true;
            if (fireEffect != null)
                fireEffect.SetActive(true);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
