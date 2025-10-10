using UnityEngine;

public class WindowToggle : MonoBehaviour
{
    [Header("Ссылка на окно (панель UI)")]
    public GameObject window;

    // Вызывается кнопкой
    public void ToggleWindow()
    {
        // Меняем состояние окна
        window.SetActive(!window.activeSelf);
    }
}
