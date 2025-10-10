using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public GameObject[] resourcePrefabs; // ������ �������� (���������, �����, ���)
    public int spawnCount = 20; // ������� �������� ���������
    public Vector3 spawnArea = new Vector3(50, 0, 50); // ���� ������ (�� X � Z)
    public float minY = 0.5f; // ������ ��� �����

    void Start()
    {
        SpawnResources();
    }

    void SpawnResources()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // �������� ��������� ������
            GameObject resourcePrefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];

            // �������� ��������� �������
            float x = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
            float z = Random.Range(-spawnArea.z / 2, spawnArea.z / 2);
            Vector3 spawnPos = new Vector3(x, minY, z) + transform.position;

            // ������ ������
            Instantiate(resourcePrefab, spawnPos, Quaternion.identity);
        }
    }
}
