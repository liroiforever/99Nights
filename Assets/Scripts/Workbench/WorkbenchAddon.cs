using UnityEngine;

[DisallowMultipleComponent]
public class WorkbenchAddon : MonoBehaviour
{
    [Tooltip("Имя, по которому верстак будет меня искать (например 'ToolShelf')")]
    public string addonName = "ToolShelf";

    [Tooltip("На какой уровень верстака влияет этот объект (пример: для апгрейда до 2 уровня)")]
    public int relatedWorkbenchLevel = 2;

    [Tooltip("Радиус, в котором верстак сможет меня 'увидеть'")]
    public float detectionRadius = 4f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
