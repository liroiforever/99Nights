using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkbenchUpgradeTooltip : MonoBehaviour
{
    public Workbench workbench; // сам верстак
    public int targetLevel = 2; // на какой уровень апгрейд
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI requirementsText;
    public PlayerInventory playerInventory;

    void Start()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
    }

    public void ShowTooltip()
    {
        if (tooltipPanel == null || workbench == null) return;

        var req = workbench.upgradeRequirements.Find(r => r.targetLevel == targetLevel);
        if (req == null) return;

        titleText.text = $"Апгрейд до уровня {targetLevel}";

        string text = "";
        // Проверка ресурсов
        foreach (var r in req.resources)
        {
            bool has = playerInventory.HasResource(r.resourceType, r.amount);
            string color = has ? "green" : "red";
            text += $"<color={color}>{r.resourceType}: {r.amount}</color>\n";
        }

        // Проверка requiredNearbyObject
        if (req.requiredNearbyObject != null)
        {
            bool found = false;
            string targetName = req.requiredNearbyObject.name;
            foreach (WorkbenchAddon addon in FindObjectsOfType<WorkbenchAddon>())
            {
                float dist = Vector3.Distance(workbench.transform.position, addon.transform.position);
                if (dist <= addon.detectionRadius && addon.addonName.Contains(targetName))
                {
                    found = true;
                    break;
                }
            }

            string color = found ? "green" : "red";
            text += $"<color={color}>Нужен рядом объект: {targetName}</color>\n";
        }

        requirementsText.text = text;
        tooltipPanel.SetActive(true);
    }


    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
