using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class VegetationSpawnerWindow : EditorWindow
{
    [Header("Основные настройки")]
    public Terrain targetTerrain;
    public int seed = 0;
    public float spawnRadius = 500f;

    [Header("Плотность (общее количество объектов каждого типа)")]
    public int treeCount = 150;
    public int bushCount = 300;
    public int grassCount = 800;

    [Header("Префабы")]
    public List<GameObject> treePrefabs = new List<GameObject>();
    public List<GameObject> bushPrefabs = new List<GameObject>();
    public List<GameObject> grassPrefabs = new List<GameObject>();

    [Header("Параметры размещения")]
    public bool preservePrefabScale = true;
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    public bool randomRotationY = true;

    [Header("Перлин-шум (естественность распределения)")]
    public float noiseScale = 50f;
    public float treeThreshold = 0.55f;
    public float bushThreshold = 0.45f;
    public float grassThreshold = 0.35f;

    // Текстура для превью шума
    private Texture2D noisePreview;
    private const int previewSize = 128;
    private Vector2 scrollPos; // для скролла тулзы

    [MenuItem("Tools/Vegetation Spawner")]
    public static void ShowWindow()
    {
        GetWindow<VegetationSpawnerWindow>("Vegetation Spawner");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

        GUILayout.Label("Настройки генерации растительности", EditorStyles.boldLabel);

        targetTerrain = (Terrain)EditorGUILayout.ObjectField("Terrain", targetTerrain, typeof(Terrain), true);
        seed = EditorGUILayout.IntField("Seed", seed);
        spawnRadius = EditorGUILayout.FloatField("Радиус спауна", spawnRadius);

        EditorGUILayout.Space();
        GUILayout.Label("Плотность", EditorStyles.boldLabel);
        treeCount = EditorGUILayout.IntField("Количество деревьев", treeCount);
        bushCount = EditorGUILayout.IntField("Количество кустов", bushCount);
        grassCount = EditorGUILayout.IntField("Количество травы/грибов", grassCount);

        EditorGUILayout.Space();
        GUILayout.Label("Префабы", EditorStyles.boldLabel);
        SerializedObject so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("treePrefabs"), true);
        EditorGUILayout.PropertyField(so.FindProperty("bushPrefabs"), true);
        EditorGUILayout.PropertyField(so.FindProperty("grassPrefabs"), true);
        so.ApplyModifiedProperties();

        EditorGUILayout.Space();
        GUILayout.Label("Параметры", EditorStyles.boldLabel);
        preservePrefabScale = EditorGUILayout.Toggle("Сохранять масштаб префаба", preservePrefabScale);
        scaleRange = EditorGUILayout.Vector2Field("Случайный масштаб", scaleRange);
        randomRotationY = EditorGUILayout.Toggle("Случайный поворот Y", randomRotationY);

        EditorGUILayout.Space();
        GUILayout.Label("Перлин-шум (распределение)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        noiseScale = EditorGUILayout.FloatField("Масштаб шума", noiseScale);
        treeThreshold = EditorGUILayout.Slider("Порог деревьев", treeThreshold, 0f, 1f);
        bushThreshold = EditorGUILayout.Slider("Порог кустов", bushThreshold, 0f, 1f);
        grassThreshold = EditorGUILayout.Slider("Порог травы/грибов", grassThreshold, 0f, 1f);
        if (EditorGUI.EndChangeCheck())
            GenerateNoisePreview();

        EditorGUILayout.Space(10);

        // Превью карты шума
        if (noisePreview == null)
            GenerateNoisePreview();

        GUILayout.Label("Превью распределения шума", EditorStyles.miniBoldLabel);
        GUILayout.Box(noisePreview, GUILayout.Width(previewSize), GUILayout.Height(previewSize));

        EditorGUILayout.Space(15);

        // Кнопка Random Seed
        if (GUILayout.Button("Random Seed"))
        {
            seed = Random.Range(0, 100000);
            ClearGenerated();
            Generate();
        }

        // Кнопка генерации (старое удаление автоматически)
        if (GUILayout.Button("Сгенерировать растительность"))
        {
            ClearGenerated();
            Generate();
        }

        if (GUILayout.Button("Очистить сгенерированные объекты"))
        {
            ClearGenerated();
        }

        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        if (!targetTerrain)
        {
            Debug.LogError("Не выбран Terrain!");
            return;
        }

        Random.InitState(seed);

        TerrainData data = targetTerrain.terrainData;
        Vector3 terrainPos = targetTerrain.transform.position;

        GameObject root = new GameObject("Vegetation_Root");

        SpawnGroup(treePrefabs, treeCount, "Trees", treeThreshold, data, terrainPos, root.transform);
        SpawnGroup(bushPrefabs, bushCount, "Bushes", bushThreshold, data, terrainPos, root.transform);
        SpawnGroup(grassPrefabs, grassCount, "Grass", grassThreshold, data, terrainPos, root.transform);

        Debug.Log($"Генерация завершена! Seed = {seed}");
    }

    private void SpawnGroup(List<GameObject> prefabs, int count, string name, float threshold, TerrainData data, Vector3 terrainPos, Transform root)
    {
        if (prefabs.Count == 0) return;

        GameObject parent = new GameObject(name);
        parent.transform.SetParent(root);

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(0f, data.size.x);
            float z = Random.Range(0f, data.size.z);

            float sampleX = (x + seed) / noiseScale;
            float sampleZ = (z + seed) / noiseScale;
            float noise = Mathf.PerlinNoise(sampleX, sampleZ);

            if (noise < threshold) continue;

            float y = data.GetInterpolatedHeight(x / data.size.x, z / data.size.z);
            Vector3 worldPos = new Vector3(x, y, z) + terrainPos;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = worldPos;

            if (randomRotationY)
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);

            if (!preservePrefabScale)
                obj.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);

            obj.transform.SetParent(parent.transform);
        }
    }

    private void ClearGenerated()
    {
        GameObject root = GameObject.Find("Vegetation_Root");
        if (root) DestroyImmediate(root);
    }

    private void GenerateNoisePreview()
    {
        if (noisePreview == null)
            noisePreview = new Texture2D(previewSize, previewSize);

        for (int x = 0; x < previewSize; x++)
        {
            for (int y = 0; y < previewSize; y++)
            {
                float nx = (float)x / (previewSize - 1);
                float ny = (float)y / (previewSize - 1);

                float n = Mathf.PerlinNoise(nx * 5f + seed * 0.01f, ny * 5f + seed * 0.01f);

                Color c = Color.black;

                if (n >= treeThreshold)
                    c = Color.red;
                else if (n >= bushThreshold)
                    c = Color.green;
                else if (n >= grassThreshold)
                    c = Color.yellow;
                else
                    c = Color.black;

                noisePreview.SetPixel(x, y, c);
            }
        }

        noisePreview.Apply();
    }
}
