using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Параметры AI")]
    public Transform player;
    public float speed = 3f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;

    [HideInInspector] public bool isActive = true;
    private float lastAttackTime;

    void OnEnable()
    {
        DayNightCycle.OnNightStart += ActivateAI;
        DayNightCycle.OnDayStart += DeactivateAI;
    }

    void OnDisable()
    {
        DayNightCycle.OnNightStart -= ActivateAI;
        DayNightCycle.OnDayStart -= DeactivateAI;
    }

    void Update()
    {
        if (!isActive || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"Игрок получил урон: {attackDamage}");
                }
                lastAttackTime = Time.time;
            }
        }
    }

    void ActivateAI()
    {
        isActive = true;
        Debug.Log($"{gameObject.name} активирован (ночь)");
    }

    void DeactivateAI()
    {
        isActive = false;
        Debug.Log($"{gameObject.name} отключён (день)");
    }
}
