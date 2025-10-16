using UnityEngine;

[ExecuteAlways]
public class BezierCurve : MonoBehaviour
{
    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;

    // bezier formula
    public Vector3 GetPoint(float t)
    {
        return Mathf.Pow(1 - t, 3) * p0.position + 3 * Mathf.Pow(1 - t, 2) * t * p1.position + 3 * (1 - t) * Mathf.Pow(t, 2) * p2.position + Mathf.Pow(t, 3) * p3.position;
    }
}
