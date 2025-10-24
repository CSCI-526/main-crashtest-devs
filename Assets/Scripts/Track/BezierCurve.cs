using UnityEngine;

[ExecuteAlways]
public class BezierCurve : MonoBehaviour
{
    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;
    public bool[] respawnSpots = new bool[10]; // false means not taken, true means taken
    public float[] respawnTimers = new float[10];

    // bezier formula
    public Vector3 GetPoint(float t)
    {
        return Mathf.Pow(1 - t, 3) * p0.position + 3 * Mathf.Pow(1 - t, 2) * t * p1.position + 3 * (1 - t) * Mathf.Pow(t, 2) * p2.position + Mathf.Pow(t, 3) * p3.position;
    }

    public Vector3 GetTangent(float t)
    {
        return 3 * Mathf.Pow(1 - t, 2) * (p1.position - p0.position) + 6 * (1 - t) * t * (p2.position - p1.position) + 3 * Mathf.Pow(t, 2) * (p3.position - p2.position);
    }

    public Vector3 GetOffsetPoint(float t, bool rightSide)
    {
        Vector3 point = GetPoint(t);
        Vector3 tangent = GetTangent(t).normalized;

        Vector3 up = Vector3.up;
        Vector3 side = Vector3.Cross(up, tangent).normalized;

        float direction = rightSide ? 1f : -1f;
        return point + 5f * direction * side;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < respawnSpots.Length; i++)
        {
            if (respawnSpots[i]) respawnTimers[i] -= Time.deltaTime;

            if (respawnTimers[i] <= 0f)
            {
                respawnTimers[i] = 0f;
                respawnSpots[i] = false;
            }
        }
    }

    public float GetClosestTOnCurve(Vector3 position)
    {
        int samples = 20;
        float closestT = 0f;
        float closestDist = float.MaxValue;

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 point = GetPoint(t);
            float dist = Vector3.SqrMagnitude(position - point);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestT = t;
            }
        }

        return closestT;
    }
}
