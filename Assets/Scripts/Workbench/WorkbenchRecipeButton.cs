using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class WorkbenchRecipeButton : MonoBehaviour
{
    [Header("Ссылки")]
    public Workbench workbench;
    public Workbench.WorkbenchType workbenchType = Workbench.WorkbenchType.Default;
    public int recipeIndex;
    public Image icon;
    public TMP_Text descriptionText;
    public TMP_Text resourcesText;
    public Image progressImage;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        // ? Авто-подхват через менеджер
        if (workbench == null)
            FindWorkbench();

        // Прогресс Image остаётся без изменений
        if (progressImage == null)
        {
            GameObject progObj = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
            progObj.transform.SetParent(transform, false);
            progressImage = progObj.GetComponent<Image>();
            progressImage.color = new Color(1f, 1f, 1f, 0.3f);
            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Horizontal;
            progressImage.fillOrigin = 0;
            progressImage.fillAmount = 0f;
            progressImage.raycastTarget = false;
            progressImage.rectTransform.anchorMin = Vector2.zero;
            progressImage.rectTransform.anchorMax = Vector2.one;
            progressImage.rectTransform.offsetMin = Vector2.zero;
            progressImage.rectTransform.offsetMax = Vector2.zero;
            progressImage.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // На случай, если верстак создавался динамически после Awake
        if (workbench == null)
            FindWorkbench();
    }

    private void FindWorkbench()
    {
        if (WorkbenchManager.Instance != null)
            workbench = WorkbenchManager.Instance.GetWorkbench(workbenchType, recipeIndex);

        if (workbench != null)
            Debug.Log($"[{name}] Подхвачен верстак '{workbench.name}' для рецепта #{recipeIndex}");
        else
            Debug.LogWarning($"[{name}] Не удалось найти верстак для рецепта #{recipeIndex}, тип {workbenchType}");
    }

    private void OnEnable()
    {
        UpdateVisualState();
        if (button != null)
            button.onClick.AddListener(OnClickCraft);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClickCraft);
    }

    public void UpdateVisualState()
    {
        if (workbench == null || recipeIndex < 0 || recipeIndex >= workbench.recipes.Count)
            return;

        var recipe = workbench.recipes[recipeIndex];
        bool available = workbench.currentLevel >= recipe.requiredWorkbenchLevel;

        if (workbench.recipeButtons != null && recipeIndex < workbench.recipeButtons.Length)
            button.interactable = available && !workbench.recipeButtons[recipeIndex].interactable;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = available ? 1f : 0.5f;

        if (icon != null)
            icon.color = available ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.7f);

        if (descriptionText != null)
            descriptionText.text = recipe.recipeData.result.itemName;

        if (resourcesText != null)
        {
            string resStr = "";
            foreach (var req in recipe.recipeData.requirements)
                resStr += $"{req.resourceType}: {req.amount}\n";
            resourcesText.text = resStr.TrimEnd('\n');
        }
    }

    private void OnClickCraft()
    {
        if (workbench == null)
        {
            FindWorkbench();
            if (workbench == null) return;
        }

        workbench.StartCraft(recipeIndex, progressImage);
    }
}
