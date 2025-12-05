using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TrackSketchLoaderNOLONGERUSING : MonoBehaviour
{
    [Header("Track Preview Settings")]
    public List<GameObject> trackPrefabs; // Assign your track prefabs here (0=straight, 3=turn)
    public float drawSpeed = 50f;
    public int pointsPerSegment = 10;

    [Header("Visual Settings")]
    public float lineWidth = 2f;
    public Color lineColor = Color.white;
    public float previewScale = 0.5f;

    private LineRenderer sketchLine;
    private TMP_Text loadingText;
    private Canvas canvas;
    private string sceneToLoad;
    private GameObject trackContainer;
    private List<GameObject> generatedSegments = new List<GameObject>();

    // Static method to set which scene to load
    public static string targetScene = "";

    private void Start()
    {
        sceneToLoad = string.IsNullOrEmpty(targetScene) ? "SinglePlayer" : targetScene;

        SetupCanvas();
        SetupTrackContainer();
        SetupLineRenderer();
        StartCoroutine(GenerateAndSketchTrack());
    }

    private void SetupTrackContainer()
    {
        trackContainer = new GameObject("PreviewTrack");
        trackContainer.transform.localScale = Vector3.one * previewScale;
    }

    private void SetupCanvas()
    {
        // Make a canvas for the loading text
        GameObject canvasObj = new GameObject("LoadingCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        // Setup the loading text
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(canvasObj.transform);
        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Generating tracks...";
        loadingText.fontSize = 48;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = Color.white;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.3f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
        rectTransform.sizeDelta = new Vector2(800, 100);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private void SetupLineRenderer()
    {
        GameObject lineObj = new GameObject("TrackSketch");
        sketchLine = lineObj.AddComponent<LineRenderer>();

        sketchLine.startWidth = lineWidth;
        sketchLine.endWidth = lineWidth;
        sketchLine.material = new Material(Shader.Find("Sprites/Default"));
        sketchLine.startColor = lineColor;
        sketchLine.endColor = lineColor;
        sketchLine.positionCount = 0;

        // Point camera down from above
        Camera.main.transform.position = new Vector3(0, 150, 0);
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

        // Use orthographic for clean top-down view
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 140;
    }

    private IEnumerator GenerateAndSketchTrack()
    {
        // Start loading the scene in the background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        if (trackPrefabs == null || trackPrefabs.Count == 0)
        {
            Debug.LogWarning("No track prefabs assigned!");
            yield return StartCoroutine(SimpleFallbackAnimation(asyncLoad));
            yield break;
        }

        List<GameObject> selectedPrefabs = SelectRandomSegments();

        int currentPoint = 0;
        BezierCurve lastCurve = null;
        BezierCurve firstCurve = null;

        for (int i = 0; i < selectedPrefabs.Count; i++)
        {
            GameObject segment = SpawnSegment(selectedPrefabs[i], lastCurve);
            generatedSegments.Add(segment);
            BezierCurve curve = segment.GetComponent<BezierCurve>();

            if (i == 0) firstCurve = curve;
            lastCurve = curve;

            // Draw the line along this segment
            for (int j = 0; j < pointsPerSegment; j++)
            {
                float t = j / (float)pointsPerSegment;
                Vector3 point = curve.GetPoint(t);

                sketchLine.positionCount = currentPoint + 1;
                sketchLine.SetPosition(currentPoint, point);
                currentPoint++;

                float progress = (i * pointsPerSegment + j) / (float)(selectedPrefabs.Count * pointsPerSegment) * 100f;
                loadingText.text = $"Generating track... {Mathf.RoundToInt(progress)}%";

                yield return new WaitForSeconds(1f / drawSpeed);
            }
        }

        loadingText.text = "Track ready!";
        yield return new WaitForSeconds(0.5f);

        // Clean up the preview
        foreach (GameObject segment in generatedSegments)
        {
            Destroy(segment);
        }
        Destroy(trackContainer);

        asyncLoad.allowSceneActivation = true;
    }

    private List<GameObject> SelectRandomSegments()
    {
        List<GameObject> selected = new List<GameObject>();

        if (trackPrefabs.Count == 0) return selected;

        int totalSegments = Random.Range(40, 60);

        for (int i = 0; i < totalSegments; i++)
        {
            int randomIndex = Random.Range(0, trackPrefabs.Count);
            selected.Add(trackPrefabs[randomIndex]);
        }

        return selected;
    }

    private GameObject SpawnSegment(GameObject prefab, BezierCurve lastCurve)
    {
        GameObject segment = Instantiate(prefab, trackContainer.transform);

        if (lastCurve != null)
        {
            BezierCurve newCurve = segment.GetComponent<BezierCurve>();

            Vector3 lastP2 = lastCurve.p2.position;
            Vector3 lastP3 = lastCurve.p3.position;
            Vector3 tangent = (lastP3 - lastP2).normalized;

            Vector3 offset = lastP3 - newCurve.p0.position;
            segment.transform.position += offset;

            Quaternion rotation = Quaternion.LookRotation(tangent, lastCurve.transform.up);
            segment.transform.rotation = rotation;
        }

        // Turn off colliders for the preview
        foreach (Collider col in segment.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        return segment;
    }

    private IEnumerator SimpleFallbackAnimation(AsyncOperation asyncLoad)
    {
        for (int i = 0; i < 100; i++)
        {
            loadingText.text = $"Generating track... {i}%";
            yield return new WaitForSeconds(0.02f);
        }

        loadingText.text = "Track ready!";
        yield return new WaitForSeconds(0.5f);
        asyncLoad.allowSceneActivation = true;
    }

    private IEnumerator FadeOutSketch()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Color startColor = sketchLine.colorGradient.colorKeys[0].color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);

            Gradient newGradient = new Gradient();
            Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            newGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(fadeColor, 0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f) }
            );
            sketchLine.colorGradient = newGradient;

            yield return null;
        }

        Destroy(sketchLine.gameObject);
    }
}
