using UnityEngine;

[ExecuteAlways]
public class BezierCurveTEST : MonoBehaviour
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
        return Mathf.Pow(1 - t, 3) * p0.position +
               3 * Mathf.Pow(1 - t, 2) * t * p1.position +
               3 * (1 - t) * Mathf.Pow(t, 2) * p2.position +
               Mathf.Pow(t, 3) * p3.position;
    }
}
