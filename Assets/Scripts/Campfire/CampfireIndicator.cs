using UnityEngine;
using UnityEngine.UI;

public class CampfireIndicator : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform player;
    public Transform campfire;
    public Camera mainCamera;
    public RectTransform arrowUI;   // стрелка на экране
    public CanvasGroup arrowGroup;  // для плавного появления

    [Header("Настройки")]
    public float edgeOffset = 60f;
    public float fadeSpeed = 4f;    // скорость плавного появления/исчезновения

    private Vector3 screenPoint;
    private Vector3 direction;
    private bool shouldShow;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (arrowGroup == null)
        {
            arrowGroup = arrowUI.GetComponent<CanvasGroup>();
            if (arrowGroup == null)
                arrowGroup = arrowUI.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (player == null || campfire == null || arrowUI == null) return;

        screenPoint = mainCamera.WorldToScreenPoint(campfire.position);
        bool isBehind = screenPoint.z < 0;

        // Проверка видимости костра
        if (!isBehind &&
            screenPoint.x > 0 && screenPoint.x < Screen.width &&
            screenPoint.y > 0 && screenPoint.y < Screen.height)
        {
            shouldShow = false;
        }
        else
        {
            shouldShow = true;
        }

        // Плавное появление / исчезновение стрелки
        float targetAlpha = shouldShow ? 1f : 0f;
        arrowGroup.alpha = Mathf.MoveTowards(arrowGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        arrowGroup.blocksRaycasts = arrowGroup.alpha > 0.01f;
        arrowGroup.interactable = arrowGroup.alpha > 0.01f;

        if (arrowGroup.alpha <= 0.01f) return;

        // Если костёр за спиной — переворачиваем
        if (isBehind)
            screenPoint *= -1;

        direction = (screenPoint - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;

        float x = Mathf.Clamp(direction.x * (Screen.width / 2f - edgeOffset) + Screen.width / 2f,
                              edgeOffset, Screen.width - edgeOffset);
        float y = Mathf.Clamp(direction.y * (Screen.height / 2f - edgeOffset) + Screen.height / 2f,
                              edgeOffset, Screen.height - edgeOffset);

        arrowUI.position = new Vector3(x, y, 0);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowUI.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}
