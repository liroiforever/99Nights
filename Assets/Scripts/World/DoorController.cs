using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    [Header("Player settings")]
    public Transform player;
    public float openRadius = 2f;

    [Header("Animation / Actions")]
    public Animator animator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    public UnityEvent OnOpenDoor;
    public UnityEvent OnCloseDoor;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    [Header("Auto-close")]
    public float closeDelay = 3f;

    [Header("Misc")]
    public bool lockWhileAnimating = true;

    bool isOpen = false;
    bool isLocked = false;
    Coroutine closeCoroutine;

    void Awake()
    {
        // Получаем или добавляем AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Автопоиск игрока по тэгу "Player", если ссылка не назначена
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogWarning("DoorController: Игрок с тэгом 'Player' не найден!");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= openRadius)
        {
            if (!isOpen) Open();
            // Отменяем таймер закрытия
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }
        }
        else
        {
            if (isOpen && closeCoroutine == null)
            {
                closeCoroutine = StartCoroutine(AutoCloseCoroutine());
            }
        }
    }

    void Open()
    {
        if (lockWhileAnimating && isLocked) return;

        isOpen = true;
        if (lockWhileAnimating) isLocked = true;

        if (animator != null)
        {
            animator.ResetTrigger(closeTrigger);
            animator.SetTrigger(openTrigger);
        }

        OnOpenDoor?.Invoke();

        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        if (lockWhileAnimating) isLocked = false;
    }

    void Close()
    {
        if (lockWhileAnimating && isLocked) return;

        isOpen = false;
        if (lockWhileAnimating) isLocked = true;

        if (animator != null)
        {
            animator.ResetTrigger(openTrigger);
            animator.SetTrigger(closeTrigger);
        }

        OnCloseDoor?.Invoke();

        if (closeSound != null)
            audioSource.PlayOneShot(closeSound);

        if (lockWhileAnimating) isLocked = false;
    }

    IEnumerator AutoCloseCoroutine()
    {
        yield return new WaitForSeconds(closeDelay);
        Close();
        closeCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, openRadius);
    }
}
