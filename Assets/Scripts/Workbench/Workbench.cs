using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Workbench : MonoBehaviour
{
    public enum WorkbenchType
    {
        Default,
        Furnace,
        CraftingTable
    }

    [Header("Тип верстака")]
    public WorkbenchType workbenchType = WorkbenchType.Default;

    [System.Serializable]
    public class Recipe
    {
        public CraftRecipeData recipeData;
        public int requiredWorkbenchLevel = 1;
    }

    [System.Serializable]
    public class RecipeRequirement
    {
        public string resourceType;
        public int amount;
    }

    [System.Serializable]
    public class UpgradeRequirement
    {
        public int targetLevel = 2;
        public List<RecipeRequirement> resources = new List<RecipeRequirement>();
        public GameObject requiredNearbyObject;
        public float detectRadius = 3f;
    }

    [Header("Основные параметры")]
    public List<Recipe> recipes = new List<Recipe>();
    public List<UpgradeRequirement> upgradeRequirements = new List<UpgradeRequirement>();
    public GameObject craftingUI;
    public PlayerInventory playerInventory;

    [Header("UI элементы")]
    public Button[] recipeButtons;
    public TMP_Text[] recipeInfoTexts;
    public TMP_Text workbenchLevelText;

    [Header("Уровни верстака")]
    public int currentLevel = 1;
    public int maxLevel = 3;

    private bool[] recipeInProgress;

    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        // ? Если UI не привязан — ищем его автоматически в сцене
        if (craftingUI == null)
            craftingUI = FindCraftingUI();

        if (craftingUI != null)
            craftingUI.SetActive(false);

        recipeInProgress = new bool[recipes.Count];
        UpdateCraftingUI();
    }

    // ======== Авто-поиск UI вне зависимости от имени, тега и активности ========
    private GameObject FindCraftingUI()
    {
        string uiName = "";
        switch (workbenchType)
        {
            case WorkbenchType.Furnace:
                uiName = "FurnacePanel";
                break;
            case WorkbenchType.CraftingTable:
                uiName = "CraftingTablePanel";
                break;
            default:
                uiName = "WorkbenchPanel";
                break;
        }

        // Поиск по всему сцене включая неактивные объекты
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name == uiName && obj.scene.IsValid())
            {
                return obj;
            }
        }

        Debug.LogWarning($"?? UI панель '{uiName}' не найдена на сцене!");
        return null;
    }

    // ======== Вход / выход игрока ========
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && craftingUI != null)
            craftingUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && craftingUI != null)
            craftingUI.SetActive(false);
    }

    // ======== Старт крафта ========
    public void StartCraft(int recipeIndex, Image progressImage)
    {
        if (!gameObject.activeInHierarchy) return;

        if (recipeIndex < 0 || recipeIndex >= recipes.Count)
        {
            Debug.LogError($"?? Workbench: Рецепт с индексом {recipeIndex} отсутствует!");
            return;
        }

        if (recipeInProgress[recipeIndex])
            return;

        StartCoroutine(CraftProcess(recipeIndex, progressImage));
    }

    // ======== Корутина крафта ========
    private IEnumerator CraftProcess(int recipeIndex, Image progressImage)
    {
        var recipe = recipes[recipeIndex].recipeData;
        var requiredLevel = recipes[recipeIndex].requiredWorkbenchLevel;

        if (currentLevel < requiredLevel)
        {
            Debug.Log($"? Требуется уровень верстака {requiredLevel} для {recipe.result.itemName}");
            yield break;
        }

        foreach (var req in recipe.requirements)
        {
            if (!playerInventory.HasResource(req.resourceType, req.amount))
            {
                Debug.Log($"? Не хватает {req.resourceType} для крафта {recipe.result.itemName}");
                yield break;
            }
        }

        recipeInProgress[recipeIndex] = true;
        if (recipeButtons != null && recipeIndex < recipeButtons.Length)
            recipeButtons[recipeIndex].interactable = false;

        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
            progressImage.gameObject.SetActive(true);
        }

        float time = recipe.craftTime;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            if (progressImage != null)
                progressImage.fillAmount = Mathf.Clamp01(elapsed / time);
            yield return null;
        }

        foreach (var req in recipe.requirements)
            playerInventory.ConsumeResource(req.resourceType, req.amount);

        playerInventory.AddItem(recipe.result);
        playerInventory.UpdateUI();
        Debug.Log($"? Создан предмет: {recipe.result.itemName}");

        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
            progressImage.gameObject.SetActive(false);
        }

        if (recipeButtons != null && recipeIndex < recipeButtons.Length)
            recipeButtons[recipeIndex].interactable = true;

        recipeInProgress[recipeIndex] = false;
    }

    // ======== Обновление UI ========
    public void UpdateCraftingUI()
    {
        if (workbenchLevelText != null)
            workbenchLevelText.text = $": {currentLevel}";

        for (int i = 0; i < recipeButtons.Length && i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            bool unlocked = currentLevel >= recipe.requiredWorkbenchLevel;
            recipeButtons[i].interactable = unlocked && !recipeInProgress[i];

            if (recipeInfoTexts != null && i < recipeInfoTexts.Length)
                recipeInfoTexts[i].text = unlocked ? "" : $"?? Уровень {recipe.requiredWorkbenchLevel}";
        }
    }

    // ======== Апгрейд верстака ========
    public void TryUpgradeWorkbench()
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log("?? Верстак уже максимального уровня!");
            return;
        }

        UpgradeRequirement req = upgradeRequirements.Find(r => r.targetLevel == currentLevel + 1);
        if (req == null)
        {
            Debug.LogWarning($"?? Нет данных для апгрейда на уровень {currentLevel + 1}");
            return;
        }

        foreach (var res in req.resources)
        {
            if (!playerInventory.HasResource(res.resourceType, res.amount))
            {
                Debug.Log($"? Не хватает ресурса {res.resourceType} для апгрейда!");
                return;
            }
        }

        if (req.requiredNearbyObject != null)
        {
            bool found = false;
            foreach (WorkbenchAddon addon in FindObjectsOfType<WorkbenchAddon>())
            {
                float dist = Vector3.Distance(transform.position, addon.transform.position);
                if (dist <= req.detectRadius && addon.addonName.Contains(req.requiredNearbyObject.name))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.Log($"? Для апгрейда требуется поблизости объект типа: {req.requiredNearbyObject.name}");
                return;
            }
        }

        foreach (var res in req.resources)
            playerInventory.ConsumeResource(res.resourceType, res.amount);

        currentLevel++;
        Debug.Log($"? Верстак улучшен до уровня {currentLevel}!");

        UpdateCraftingUI();
    }
    private void OnEnable()
    {
        WorkbenchManager.Instance?.RegisterWorkbench(this);
    }

    private void OnDisable()
    {
        WorkbenchManager.Instance?.UnregisterWorkbench(this);
    }

}
