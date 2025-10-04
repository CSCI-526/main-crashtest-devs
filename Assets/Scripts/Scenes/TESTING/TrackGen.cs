using System.Collections.Generic;
using UnityEngine;

public class TrackGen : MonoBehaviour
{
    [Header("Track Prefabs (different curve shapes)")]
    public List<GameObject> trackPrefabs; // Assign different segment prefabs in the Inspector

    [Header("Starting Track")]
    public BezierCurveTEST startTrack;

    [Header("Number of segments to generate")]
    public int segmentsToGenerate = 5;

    private BezierCurveTEST lastCurve; // The most recently placed segment
    private int slope = 0;

    void Start()
    {
        if (startTrack == null || trackPrefabs.Count == 0)
        {
            Debug.LogError("TrackGenerator: Missing startTrack or trackPrefabs!");
            return;
        }

        lastCurve = startTrack;

        for (int i = 0; i < segmentsToGenerate; i++)
        {
            SpawnNextSegment();
        }
    }

    void SpawnNextSegment()
    {
        GameObject prefab = PickSegment();
        GameObject newSegment = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        BezierCurveTEST newCurve = newSegment.GetComponent<BezierCurveTEST>();

        if (!newCurve)
        {
            Debug.LogError("TrackGenerator: Segment prefab missing BezierCurveTEST script.");
            return;
        }

        Vector3 lastP2 = lastCurve.p2.position;
        Vector3 lastP3 = lastCurve.p3.position;

        Vector3 tangent = (lastP3 - lastP2).normalized;

        Vector3 offset = lastP3 - newCurve.p0.position;
        newSegment.transform.position += offset;

        Quaternion rotation = Quaternion.LookRotation(tangent, lastCurve.transform.up);
        newSegment.transform.rotation = rotation;

        if (newSegment.TryGetComponent<ReadMeshTEST>(out var roadMesh))
        {
            roadMesh.GenerateRoad();
        }

        lastCurve = newCurve;
    }

    GameObject PickSegment()
    {
        GameObject segment;

        switch (slope)
        {
            case 1:
                if (Random.Range(0, 2) == 0) segment = trackPrefabs[0];
                else { segment = trackPrefabs[2]; slope--; }
                break;
            case -1:
                if (Random.Range(0, 2) == 0) segment = trackPrefabs[0];
                else { segment = trackPrefabs[1]; slope++; }
                break;
            default:
                segment = trackPrefabs[Random.Range(0, trackPrefabs.Count)];
                if (segment.name == "up Variant") slope++;
                else if (segment.name == "down Variant") slope--;
                break;
        }


        return segment;
    }
}
