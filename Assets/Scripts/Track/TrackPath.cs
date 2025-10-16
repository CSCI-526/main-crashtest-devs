using System.Collections.Generic;
using UnityEngine;

public class TrackPath : MonoBehaviour
{
    public GameObject raceTrack;
    public List<BezierCurve> curves = new();
    private float[] curveLengths;
    private float totalLength;

    private void Awake()
    {
        for (int i = 0; i < raceTrack.transform.childCount; i++)
        {
            curves.Add(raceTrack.transform.GetChild(i).GetComponent<RoadMesh>().curve);
        }
        //curves = new List<BezierCurve>(GetComponentsInChildren<BezierCurve>());
        //curves.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        RecalcLengths();
    }

    public void RecalcLengths(int samplesPerCurve = 20)
    {
        int n = curves.Count;
        curveLengths = new float[n];
        totalLength = 0f;
        for (int i = 0; i < n; i++)
        {
            if (curves[i] == null) continue;
            curveLengths[i] = curves[i].GetLength(samplesPerCurve);
            totalLength += curveLengths[i];
        }
    }

    // t is 0..1 over entire path
    public Vector3 GetPoint(float t)
    {
        t = Mathf.Repeat(t, 1f); // allow wrapping
        if (curves.Count == 0) return transform.position;

        float dist = t * totalLength;
        for (int i = 0; i < curves.Count; i++)
        {
            if (dist <= curveLengths[i] || i == curves.Count - 1)
            {
                float localT = curveLengths[i] <= 0f ? 0f : Mathf.Clamp01(dist / curveLengths[i]);
                return curves[i].GetPoint(localT);
            }
            dist -= curveLengths[i];
        }
        return curves[curves.Count - 1].GetPoint(1f);
    }

    // tangent (direction) at t
    public Vector3 GetTangent(float t)
    {
        t = Mathf.Repeat(t, 1f);
        if (curves.Count == 0) return transform.forward;

        float dist = t * totalLength;
        for (int i = 0; i < curves.Count; i++)
        {
            if (dist <= curveLengths[i] || i == curves.Count - 1)
            {
                float localT = curveLengths[i] <= 0f ? 0f : Mathf.Clamp01(dist / curveLengths[i]);
                return curves[i].GetTangent(localT).normalized;
            }
            dist -= curveLengths[i];
        }
        return curves[curves.Count - 1].GetTangent(1f).normalized;
    }

    public float TotalLength => totalLength;

    public BezierCurve GetCurve(int index)
    {
        return curves[index];
    }

    public int GetCurveCount()
    {
        return curves.Count; 
    }
}
