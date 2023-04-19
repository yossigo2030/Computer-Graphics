using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BezierMesh : MonoBehaviour
{
    private BezierCurve curve; // The Bezier curve around which to build the mesh

    public float Radius = 0.5f; // The distance of mesh vertices from the curve
    public int NumSteps = 16; // Number of points along the curve to sample
    public int NumSides = 8; // Number of vertices created at each point

    // Awake is called when the script instance is being loaded
    public void Awake()
    {
        curve = GetComponent<BezierCurve>();
        BuildMesh();
    }
    
    /*public static Mesh GetBezierMesh(BezierCurve curve, float radius, int numSteps, int numSides)
    {
        QuadMeshData meshData = new QuadMeshData();
        var circlePoints = new List<Vector2>();
        for (int i=0; i< numSides; i++)
        {
            var angle = i * 360 / numSides;
            circlePoints.Add(GetUnitCirclePoint(angle));
        }
        // Your implementation here...
        for (int i = 0; i < numSteps + 1; i++)
        {
            var tI =(float) i / (float) numSteps;
            var sampleI = curve.GetPoint(tI);
            var normalI = curve.GetNormal(tI).normalized;
            var biNormalI = curve.GetBinormal(tI).normalized;
            for (int j = 0; j < numSides; j++)
            {
                var vertex = (circlePoints[j].x * normalI + circlePoints[j].y * biNormalI) * radius + sampleI;
                meshData.vertices.Add(vertex);
                if (i != numSteps) // no need to add a mesh beyond numSteps, only vertexes
                {
                    var v1 = i * numSides + j;
                    var v2 = i * numSides + (j + 1) % numSides;
                    var v3 = (i + 1) * numSides + (j + 1) % numSides;
                    var v4 = (i + 1) * numSides + j;
                    meshData.quads.Add(new Vector4(v1, v2, v3, v4));
                }
            }
        }
        return meshData.ToUnityMesh();
    }*/
    
    
    public static Mesh GetBezierMesh(BezierCurve curve, float radius, int numSteps, int numSides)
    {
        QuadMeshData meshData = new QuadMeshData();
        for (int i = 0; i < numSteps + 1; i++)
        {
            PointToDrawCircle(meshData, curve, radius, numSteps, numSides, i);
            
        }
        return meshData.ToUnityMesh();
    }

    private static void PointToDrawCircle(QuadMeshData meshData, BezierCurve curve, float radius, int numSteps, int numSides, int i)
    {
        float t = i / (float) numSteps;
        Vector3 pointOfT = curve.GetPoint(t);
        Vector3 b = curve.GetBinormal(t).normalized;
        Vector3 n = curve.GetNormal(t).normalized;
        ConstructMesh(meshData, radius, numSteps, numSides, i, n, b, pointOfT);


    }

    private static void ConstructMesh(QuadMeshData meshData, float radius, int numSteps, int numSides, int i, Vector3 n, Vector3 b, Vector3 pointOfT)
    {
        for (int j = 0; j < numSides; j++)
        {
            float div = j * 360f / numSides;
            Vector2 circlePoint = GetUnitCirclePoint(div);
            Vector3 direcrtion = (circlePoint[0] * n + circlePoint[1] * b);
            Vector3 newVertex = pointOfT + direcrtion * radius;
            meshData.vertices.Add(newVertex);
            if (i < numSteps)
            {
                int x = i * numSides + j;
                int y = i * numSides + (j + 1) % numSides;
                int z = (i + 1) * numSides + (j + 1) % numSides;
                int w = (i + 1) * numSides + j;
                meshData.quads.Add(new Vector4(x, y, z, w));
            }
        }
    }
    


    // Returns 2D coordinates of a point on the unit circle at a given angle from the x-axis
    private static Vector2 GetUnitCirclePoint(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }
    

    public void BuildMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = GetBezierMesh(curve, Radius, NumSteps, NumSides);
    }

    // Rebuild mesh when BezierCurve component is changed
    public void CurveUpdated()
    {
        BuildMesh();
    }
}



[CustomEditor(typeof(BezierMesh))]
class BezierMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Update Mesh"))
        {
            var bezierMesh = target as BezierMesh;
            bezierMesh.BuildMesh();
        }
    }
}