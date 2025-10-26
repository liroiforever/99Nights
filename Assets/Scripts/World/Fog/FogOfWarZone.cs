using UnityEngine;

public class FogOfWarZone : MonoBehaviour
{
    [Header("Туман (Shader)")]
    public Material fogMaterial;
    public float currentVisibleRadius = 0f;
    public float targetVisibleRadius = 5f;
    public float maxRadius = 40f;
    public float revealSpeed = 2f;

    void Start()
    {
        UpdateShader();
    }

    void Update()
    {
        if (currentVisibleRadius < targetVisibleRadius)
        {
            currentVisibleRadius = Mathf.Lerp(currentVisibleRadius, targetVisibleRadius, Time.deltaTime * revealSpeed);
            UpdateShader();
        }
    }

    public void ExpandFog(float newRadius)
    {
        targetVisibleRadius = Mathf.Clamp(newRadius, 0f, maxRadius);
    }

    void UpdateShader()
    {
        if (fogMaterial != null)
        {
            fogMaterial.SetVector("_Center", transform.position);
            fogMaterial.SetFloat("_Radius", currentVisibleRadius);
        }
    }
}
