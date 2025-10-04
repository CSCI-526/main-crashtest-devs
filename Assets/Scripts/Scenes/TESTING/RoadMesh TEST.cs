using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ReadMeshTEST : MonoBehaviour
{
    public BezierCurveTEST curve;
    public int resolution = 50;
    public float roadWidth = 4f;

    private Mesh mesh;

    void Start()
    {
        GenerateRoad();
        AssignColliderMesh();
    }

    void AssignColliderMesh()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null && mesh != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
            mc.convex = false;
        }
    }


    void GenerateRoad()
    {
        if (curve == null)
        {
            Debug.LogWarning("ReadMeshTEST: curve is null.");
            return;
        }

        if (resolution < 2)
        {
            Debug.LogWarning("ReadMeshTEST: resolution must be >= 2.");
            return;
        }

        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        else mesh.Clear();


        Vector3[] vertices = new Vector3[(resolution + 1) * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[resolution * 6];

        Vector3 up = Vector3.up;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 center = curve.GetPoint(t);

            // tangent forward
            float delta = 1f / resolution;
            Vector3 prev = curve.GetPoint(Mathf.Max(0f, t - delta));
            Vector3 next = curve.GetPoint(Mathf.Min(1f, t + delta));
            Vector3 forward = (next - prev).normalized;


            // stable right vector (perpendicular to forward, using world up)
            Vector3 right = Vector3.Cross(forward, up).normalized;

            Vector3 leftPos = center + right * (roadWidth / 2f);
            Vector3 rightPos = center - right * (roadWidth / 2f);

            // convert into local space (mesh vertices should be in local)
            vertices[i * 2 + 0] = transform.InverseTransformPoint(leftPos);
            vertices[i * 2 + 1] = transform.InverseTransformPoint(rightPos);

            // basic UV: x along length, y across width
            float u = i / (float)resolution;
            uvs[i * 2 + 0] = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);

            // triangles
            if (i < resolution)
            {
                int triIndex = i * 6;
                int vertIndex = i * 2;

                triangles[triIndex + 0] = vertIndex + 0;
                triangles[triIndex + 1] = vertIndex + 2;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 5] = vertIndex + 3;
            }
        }

        // assign mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // assign to MeshFilter (use sharedMesh so it shows in editor)
        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        // assign to MeshCollider
        var mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            // reset before assigning (helps editor update)
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;

            // For a static track, keep convex = false.
            mc.convex = false;
        }
        else
        {
            Debug.LogWarning("ReadMeshTEST: MeshCollider missing.");
        }
    }
}
