using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChristmasLights : MonoBehaviour
{
    [Header("Color Cycle Settings")]
    public List<Color> cycleColors = new();
    public float colorChangeInterval = 2f;

    [Header("Flicker Settings")]
    public float flickerMinIntensity = 0.5f;
    public float flickerMaxIntensity = 1.2f;
    public float flickerSpeed = 15f;

    [Header("Mesh Material (for the bulb glow)")]
    public Material bulbMaterial;
    private List<LightBulb> bulbs = new();

    void Start()
    {
        InitializeBulbs();

        foreach (var bulb in bulbs)
            StartCoroutine(RunBulb(bulb));
    }

    void InitializeBulbs()
    {
        bulbs.Clear();

        foreach (Transform child in transform)
        {
            Transform pointLightObj = child.Find("Point Light");
            Light lightComp = pointLightObj.GetComponent<Light>();
            Renderer meshRenderer = child.GetComponent<Renderer>();

            Material matInstance = new (bulbMaterial);
            meshRenderer.material = matInstance;

            bulbs.Add(new LightBulb
            {
                light = lightComp,
                meshRenderer = meshRenderer,
                material = matInstance,
                colorIndex = Random.Range(0, cycleColors.Count),
                colorOffsetTime = Random.Range(0f, 2f)
            });
        }
    }

    IEnumerator RunBulb(LightBulb bulb)
    {
        float timer = bulb.colorOffsetTime;

        while (true)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, bulb.colorOffsetTime);
            float intensity = Mathf.Lerp(flickerMinIntensity, flickerMaxIntensity, noise);
            bulb.light.intensity = intensity;

            timer += Time.deltaTime;
            if (timer >= colorChangeInterval)
            {
                timer = 0f;
                bulb.colorIndex = (bulb.colorIndex + 1) % cycleColors.Count;

                Color newColor = cycleColors[bulb.colorIndex];
                bulb.light.color = newColor;

                bulb.material.SetColor("_Color", newColor);
                bulb.material.SetColor("_EmissionColor", newColor);
                bulb.material.EnableKeyword("_EMISSION");
            }

            yield return null;
        }
    }

    private class LightBulb
    {
        public Light light;
        public Renderer meshRenderer;
        public Material material;
        public int colorIndex;
        public float colorOffsetTime;
    }
}
