using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public GameObject[] resourcePrefabs; // массив ресурсов (древесина, камни, еда)
    public int spawnCount = 20; // сколько ресурсов спавнится
    public Vector3 spawnArea = new Vector3(50, 0, 50); // зона спавна (по X и Z)
    public float minY = 0.5f; // высота над землёй

    void Start()
    {
        SpawnResources();
    }

    void SpawnResources()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // выбираем случайный ресурс
            GameObject resourcePrefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];

            // выбираем случайную позицию
            float x = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
            float z = Random.Range(-spawnArea.z / 2, spawnArea.z / 2);
            Vector3 spawnPos = new Vector3(x, minY, z) + transform.position;

            // создаём ресурс
            Instantiate(resourcePrefab, spawnPos, Quaternion.identity);
        }
    }
}
