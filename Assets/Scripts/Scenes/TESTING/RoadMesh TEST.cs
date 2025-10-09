using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ReadMeshTEST : MonoBehaviour
{
    public BezierCurveTEST curve;
    public int resolution = 50;
    public float roadWidth = 35f;
    public float wallHeight = 5f;

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

    public void GenerateRoad()
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

        Vector3 up = Vector3.up;

        // ROAD BASE (flat surface)
        Vector3[] baseVertices = new Vector3[(resolution + 1) * 2];
        Vector2[] baseUVs = new Vector2[baseVertices.Length];
        int[] baseTriangles = new int[resolution * 6];

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

            baseVertices[i * 2 + 0] = transform.InverseTransformPoint(leftPos);
            baseVertices[i * 2 + 1] = transform.InverseTransformPoint(rightPos);

            float u = i / (float)resolution;
            baseUVs[i * 2 + 0] = new Vector2(u, 0f);
            baseUVs[i * 2 + 1] = new Vector2(u, 1f);

            if (i < resolution)
            {
                int triIndex = i * 6;
                int vertIndex = i * 2;

                baseTriangles[triIndex + 0] = vertIndex + 0;
                baseTriangles[triIndex + 1] = vertIndex + 2;
                baseTriangles[triIndex + 2] = vertIndex + 1;

                baseTriangles[triIndex + 3] = vertIndex + 1;
                baseTriangles[triIndex + 4] = vertIndex + 2;
                baseTriangles[triIndex + 5] = vertIndex + 3;
            }
        }

        // WALLS (extruded sides)
        // Each side has (resolution + 1) * 2 vertices and resolution * 6 triangles
        Vector3[] wallVertices = new Vector3[(resolution + 1) * 4];
        Vector2[] wallUVs = new Vector2[wallVertices.Length];
        int[] wallTriangles = new int[resolution * 12]; // 6 per side per segment

        for (int i = 0; i <= resolution; i++)
        {
            // Left wall bottom/top
            Vector3 leftBottom = baseVertices[i * 2 + 0];
            Vector3 leftTop = leftBottom + Vector3.up * wallHeight;

            // Right wall bottom/top
            Vector3 rightBottom = baseVertices[i * 2 + 1];
            Vector3 rightTop = rightBottom + Vector3.up * wallHeight;

            wallVertices[i * 4 + 0] = leftBottom;
            wallVertices[i * 4 + 1] = leftTop;
            wallVertices[i * 4 + 2] = rightBottom;
            wallVertices[i * 4 + 3] = rightTop;

            // Simple UVs (stretch vertically)
            float u = i / (float)resolution;
            wallUVs[i * 4 + 0] = new Vector2(u, 0f);
            wallUVs[i * 4 + 1] = new Vector2(u, 1f);
            wallUVs[i * 4 + 2] = new Vector2(u, 0f);
            wallUVs[i * 4 + 3] = new Vector2(u, 1f);

            if (i < resolution)
            {
                int triIndex = i * 12;
                int vertIndex = i * 4;

                // Left wall
                wallTriangles[triIndex + 0] = vertIndex + 0;
                wallTriangles[triIndex + 1] = vertIndex + 1;
                wallTriangles[triIndex + 2] = vertIndex + 5;

                wallTriangles[triIndex + 3] = vertIndex + 0;
                wallTriangles[triIndex + 4] = vertIndex + 5;
                wallTriangles[triIndex + 5] = vertIndex + 4;

                // Right wall
                wallTriangles[triIndex + 6] = vertIndex + 2;
                wallTriangles[triIndex + 7] = vertIndex + 7;
                wallTriangles[triIndex + 8] = vertIndex + 3;

                wallTriangles[triIndex + 9] = vertIndex + 2;
                wallTriangles[triIndex + 10] = vertIndex + 6;
                wallTriangles[triIndex + 11] = vertIndex + 7;
            }
        }

        // COMBINE ROAD + WALLS
        Vector3[] vertices = new Vector3[baseVertices.Length + wallVertices.Length];
        baseVertices.CopyTo(vertices, 0);
        wallVertices.CopyTo(vertices, baseVertices.Length);

        Vector2[] uvs = new Vector2[baseUVs.Length + wallUVs.Length];
        baseUVs.CopyTo(uvs, 0);
        wallUVs.CopyTo(uvs, baseUVs.Length);

        int[] triangles = new int[baseTriangles.Length + wallTriangles.Length];
        baseTriangles.CopyTo(triangles, 0);
        for (int i = 0; i < wallTriangles.Length; i++)
            triangles[baseTriangles.Length + i] = wallTriangles[i] + baseVertices.Length;

        // assign mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
            mc.convex = false;
        }
    }
}
