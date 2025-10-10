using UnityEngine;
using UnityEngine.UI;

public class Campfire : MonoBehaviour
{
    [Header("��������� ��������������")]
    public int healthPerSecond = 5;
    public int energyPerSecond = 10;
    public float tickRate = 1f;

    [Header("��������� ������")]
    public float maxBurnTime = 60f; // ������� ������ ����� ������
    private float currentBurnTime;

    [Header("UI ������")]
    public Slider burnSlider; // ������ �� �������
    public GameObject fireEffect; // particle system (�����)

    [Header("������������ ����")]
    public KeyCode addFuelKey = KeyCode.E;  // ������� ������������
    public float fuelAddAmount = 20f;       // ������� ������ ��������� ���� ������
    public float interactionRadius = 3f;    // ������ ��������������

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

        // ������� ������
        currentBurnTime -= Time.deltaTime;
        if (burnSlider != null)
            burnSlider.value = currentBurnTime;

        if (currentBurnTime <= 0)
            Extinguish();

        // ��������� �������������� � �������
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
            playerStats.SetNearCampfire(true); // <-- ����� ������

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
            playerStats.SetNearCampfire(false); // <-- ����� ����� �� ����
    }


    void TryAddFuel()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("��� ������ �� PlayerInventory!");
            return;
        }

        // ��������� ������� ������
        if (playerInventory.resources.ContainsKey("Wood") && playerInventory.resources["Wood"] > 0)
        {
            playerInventory.resources["Wood"] -= 1; // -1 ������
            playerInventory.UpdateUI(); // ������������ ���������� UI
            AddFuel(fuelAddAmount);
            Debug.Log("������: ��������� 1 ������, +20 ������ �������.");
        }
        else
        {
            Debug.Log("��� ���� ��� ������!");
        }
    }

    void Extinguish()
    {
        isActive = false;
        if (fireEffect != null)
            fireEffect.SetActive(false);
        Debug.Log("������ �����!");
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
