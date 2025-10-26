using UnityEngine;

[DisallowMultipleComponent]
public class WorkbenchAddon : MonoBehaviour
{
    [Tooltip("���, �� �������� ������� ����� ���� ������ (�������� 'ToolShelf')")]
    public string addonName = "ToolShelf";

    [Tooltip("�� ����� ������� �������� ������ ���� ������ (������: ��� �������� �� 2 ������)")]
    public int relatedWorkbenchLevel = 2;

    [Tooltip("������, � ������� ������� ������ ���� '�������'")]
    public float detectionRadius = 4f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
