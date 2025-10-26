using System.Collections.Generic;
using UnityEngine;

public class WorkbenchManager : MonoBehaviour
{
    public static WorkbenchManager Instance { get; private set; }

    private List<Workbench> allWorkbenches = new List<Workbench>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterWorkbench(Workbench wb)
    {
        if (!allWorkbenches.Contains(wb))
            allWorkbenches.Add(wb);
    }

    public void UnregisterWorkbench(Workbench wb)
    {
        if (allWorkbenches.Contains(wb))
            allWorkbenches.Remove(wb);
    }

    public Workbench GetWorkbench(Workbench.WorkbenchType type, int recipeIndex)
    {
        foreach (var wb in allWorkbenches)
        {
            if (wb.workbenchType == type &&
                recipeIndex >= 0 && recipeIndex < wb.recipes.Count)
                return wb;
        }

        foreach (var wb in allWorkbenches)
        {
            if (recipeIndex >= 0 && recipeIndex < wb.recipes.Count)
                return wb;
        }

        return null;
    }
}
