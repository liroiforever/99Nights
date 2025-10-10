using UnityEngine;

public class WindowToggle : MonoBehaviour
{
    [Header("������ �� ���� (������ UI)")]
    public GameObject window;

    // ���������� �������
    public void ToggleWindow()
    {
        // ������ ��������� ����
        window.SetActive(!window.activeSelf);
    }
}
