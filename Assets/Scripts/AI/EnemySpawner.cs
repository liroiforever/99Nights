using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnRadius = 30f;
    public float respawnInterval = 10f;

    [Header("Ссылки")]
    public Transform player;
    public DayNightCycle cycle;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool canSpawn = false;
    private float nextSpawnTime = 0f;

    void OnEnable()
    {
        DayNightCycle.OnNightStart += StartSpawning;
        DayNightCycle.OnDayStart += StopSpawning;
    }

    void OnDisable()
    {
        DayNightCycle.OnNightStart -= StartSpawning;
        DayNightCycle.OnDayStart -= StopSpawning;
    }

    void Update()
    {
        if (!canSpawn || enemyPrefab == null || player == null) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemies();
            nextSpawnTime = Time.time + respawnInterval;
        }
    }

    void StartSpawning()
    {
        if (cycle == null)
            cycle = FindObjectOfType<DayNightCycle>();

        canSpawn = true;
        SpawnEnemies();
        Debug.Log("Спавн врагов начался");
    }

    void StopSpawning()
    {
        canSpawn = false;
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.SetActive(false);
        }
        Debug.Log("День — враги исчезают");
    }

    void SpawnEnemies()
    {
        activeEnemies.RemoveAll(e => e == null);

        int toSpawn = maxEnemies - activeEnemies.Count;
        for (int i = 0; i < toSpawn; i++)
        {
            Vector3 spawnPos = player.position + (Random.insideUnitSphere * spawnRadius);
            spawnPos.y = 0;

            GameObject enemy = GetOrCreateEnemy(spawnPos);
            activeEnemies.Add(enemy);
        }
    }

    GameObject GetOrCreateEnemy(Vector3 position)
    {
        foreach (var e in activeEnemies)
        {
            if (!e.activeSelf)
            {
                e.transform.position = position;
                e.SetActive(true);
                return e;
            }
        }

        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        var ai = newEnemy.GetComponent<EnemyAI>();
        if (ai != null && player != null)
            ai.player = player;
        return newEnemy;
    }
}
