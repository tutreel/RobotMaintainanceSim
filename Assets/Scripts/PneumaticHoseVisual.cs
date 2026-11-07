
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PneumaticHoseVisual : MonoBehaviour
{
    public Transform[] points;
    public ColliderTest gripperController;

    [Header("Tube Settings")]
    public float radius = 0.008f;
    public int radialSegments = 8;
    public int segmentsPerCurve = 10;
    public float faultGap = 0.15f;

    private Mesh hoseMesh;

    void Awake()
    {
        hoseMesh = new Mesh();
        hoseMesh.name = "Pneumatic Hose Mesh";

        GetComponent<MeshFilter>().mesh = hoseMesh;
    }

    void LateUpdate()
    {
        if (points == null || points.Length < 2)
            return;

        foreach (Transform point in points)
            if (point == null) return;

        Vector3[] control = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
            control[i] = points[i].position;

        // Disconnect hose from gripper fitting
        if (gripperController != null &&
            gripperController.gripperFault)
        {
            Vector3 hoseEnd = control[control.Length - 1];
            Vector3 previousPoint = control[control.Length - 2];

            Vector3 hoseDirection =
                (hoseEnd - previousPoint).normalized;

            control[control.Length - 1] =
                hoseEnd - hoseDirection * faultGap;
        }

        // Generate smooth centerline.
        int lengthSegments =
            (points.Length - 1) * segmentsPerCurve;

        Vector3[] center = new Vector3[lengthSegments + 1];

        int index = 0;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p0 = control[Mathf.Max(i - 1, 0)];
            Vector3 p1 = control[i];
            Vector3 p2 = control[i + 1];
            Vector3 p3 = control[Mathf.Min(i + 2, points.Length - 1)];

            for (int j = 0; j < segmentsPerCurve; j++)
            {
                float t = j / (float)segmentsPerCurve;
                center[index++] = CatmullRom(p0, p1, p2, p3, t);
            }
        }

        center[index] = control[control.Length - 1];

        // Build cylindrical mesh around the centerline.
        int rings = center.Length;
        int sides = Mathf.Max(6, radialSegments);

        Vector3[] vertices = new Vector3[rings * sides];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[(rings - 1) * sides * 6];

        for (int i = 0; i < rings; i++)
        {
            Vector3 tangent;

            if (i == 0)
                tangent = center[1] - center[0];
            else if (i == rings - 1)
                tangent = center[i] - center[i - 1];
            else
                tangent = center[i + 1] - center[i - 1];

            tangent.Normalize();

            Vector3 normal =
                Vector3.Cross(tangent, Vector3.up).normalized;

            if (normal.sqrMagnitude < 0.001f)
                normal = Vector3.Cross(tangent, Vector3.right).normalized;

            Vector3 binormal =
                Vector3.Cross(tangent, normal).normalized;

            for (int j = 0; j < sides; j++)
            {
                float angle = j * Mathf.PI * 2f / sides;

                Vector3 offset =
                    normal * Mathf.Cos(angle) * radius +
                    binormal * Mathf.Sin(angle) * radius;

                vertices[i * sides + j] =
                    transform.InverseTransformPoint(center[i] + offset);

                uv[i * sides + j] =
                    new Vector2(j / (float)sides, i / (float)rings);
            }
        }

        int tri = 0;

        for (int i = 0; i < rings - 1; i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int a = i * sides + j;
                int b = i * sides + (j + 1) % sides;
                int c = (i + 1) * sides + j;
                int d = (i + 1) * sides + (j + 1) % sides;

                triangles[tri++] = a;
                triangles[tri++] = c;
                triangles[tri++] = b;

                triangles[tri++] = b;
                triangles[tri++] = c;
                triangles[tri++] = d;
            }
        }

        hoseMesh.Clear();
        hoseMesh.vertices = vertices;
        hoseMesh.uv = uv;
        hoseMesh.triangles = triangles;
        hoseMesh.RecalculateNormals();
        hoseMesh.RecalculateBounds();
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1,
                        Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f*p0 - 5f*p1 + 4f*p2 - p3) * t*t +
            (-p0 + 3f*p1 - 3f*p2 + p3) * t*t*t
        );
    }
}
