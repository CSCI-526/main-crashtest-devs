using UnityEngine;

[ExecuteAlways]
public class BezierCurve : MonoBehaviour
{
    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;

    
    private void OnDrawGizmos()
    {
        if (!p0 || !p1 || !p2 || !p3)
            return;

        // control lines
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(p0.position, p1.position);
        Gizmos.DrawLine(p2.position, p3.position);

        // Bezier curve
        Gizmos.color = Color.yellow;
        Vector3 prevPoint = p0.position;

        for (int i = 1; i <= 30; i++)
        {
            float t = i / 30f;
            Vector3 point = GetPoint(t);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    // bezier formula
    public Vector3 GetPoint(float t)
    {
        return Mathf.Pow(1 - t, 3) * p0.position + 3 * Mathf.Pow(1 - t, 2) * t * p1.position + 3 * (1 - t) * Mathf.Pow(t, 2) * p2.position + Mathf.Pow(t, 3) * p3.position;
    }

    // derivative (tangent) of cubic bezier — not normalized
    public Vector3 GetTangent(float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1 - t;

        // derivative formula: 3 * [ (1-t)^2 (p1-p0) + 2(1-t)t (p2-p1) + t^2 (p3-p2) ]
        Vector3 term1 = u * u * (p1.position - p0.position);
        Vector3 term2 = 2f * u * t * (p2.position - p1.position);
        Vector3 term3 = t * t * (p3.position - p2.position);

        return 3f * (term1 + term2 + term3);
    }

    // approximate length by sampling N segments
    public float GetLength(int samples = 20)
    {
        float length = 0f;
        Vector3 prev = GetPoint(0f);
        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector3 cur = GetPoint(t);
            length += Vector3.Distance(prev, cur);
            prev = cur;
        }
        return length;
    }
}
