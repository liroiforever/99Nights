// SpawnPoint.cs
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint Instance;

    void Awake()
    {
        // Singleton для легкого доступа
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
