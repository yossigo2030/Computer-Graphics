using System;
using System.Collections.Generic;
using UnityEngine;


public class CCMeshData
{
    public List<Vector3> points; // Original mesh points
    public List<Vector4> faces; // Original mesh quad faces
    public List<Vector4> edges; // Original mesh edges
    public List<Vector3> facePoints; // Face points, as described in the Catmull-Clark algorithm
    public List<Vector3> edgePoints; // Edge points, as described in the Catmull-Clark algorithm
    public List<Vector3> newPoints; // New locations of the original mesh points, according to Catmull-Clark
}

public class Vec2Comparer : EqualityComparer<Vector2>
{
    private static readonly float EPSILON = 0.00001f;

    public override bool Equals(Vector2 v1, Vector2 v2)
    {
        Vector2 opossiteV1 = new Vector2(v1.y, v1.x);
        if ((Vector2.Distance(v1, v2) < EPSILON) || (Vector2.Distance(opossiteV1, v2)) < EPSILON)
        {
            return true;
        }
        return false;
    }
    public override int GetHashCode(Vector2 v)
    {
        return v[0].GetHashCode() ^ v[1].GetHashCode();
    }
}


public static class CatmullClark
{
    // Returns a QuadMeshData representing the input mesh after one iteration of Catmull-Clark subdivision.
    public static QuadMeshData Subdivide(QuadMeshData quadMeshData)
    {
        // Create and initialize a CCMeshData corresponding to the given QuadMeshData
        CCMeshData meshData = new CCMeshData();
        meshData.points = quadMeshData.vertices;
        meshData.faces = quadMeshData.quads;
        meshData.edges = GetEdges(meshData);
        meshData.facePoints = GetFacePoints(meshData);
        meshData.edgePoints = GetEdgePoints(meshData);
        meshData.newPoints = GetNewPoints(meshData);

        // Combine facePoints, edgePoints and newPoints into a subdivided QuadMeshData

        List<Vector3> points = createQuadPoints(meshData);
        List<Vector4> faces = createQuadFaces(meshData);
        // Your implementation here...
        return new QuadMeshData(points, faces);
    }

    private static List<Vector4> createQuadFaces(CCMeshData meshData)
    {
        Dictionary<int, List<int>> dict = new Dictionary<int, List<int>>();
        for (int i = 0; i < meshData.edges.Count; i++)
        {
            if (!dict.ContainsKey((int)meshData.edges[i].z))
            {
                List<int> lst = new List<int>();
                dict.Add((int)meshData.edges[i].z, lst);
            }
            if (!dict.ContainsKey((int)meshData.edges[i].w))
            {
                List<int> lst = new List<int>();
                dict.Add((int)meshData.edges[i].w, lst);
            }
            dict[(int)meshData.edges[i].z].Add(i);
            dict[(int)meshData.edges[i].w].Add(i);
        }
        
        List<Vector4> quadFaces = new List<Vector4>();
        Vector4 quadFace = new Vector4();
        Vector4 quadFace1 = new Vector4();
        bool[] array = new bool[meshData.edges.Count];
        for (int i = 0; i < meshData.facePoints.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                quadFace.x = i;
                quadFace.y = dict[i][j] + meshData.facePoints.Count;
                if (!array[dict[i][j]])
                {
                    quadFace.z = meshData.edges[dict[i][j]][0] + meshData.facePoints.Count + meshData.edges.Count;
                    array[dict[i][j]] = true;
                }
                else
                {
                    quadFace.z = meshData.edges[dict[i][j]][1] + meshData.facePoints.Count + meshData.edges.Count;
                }
                foreach (var index in dict[i])
                {
                    if (((int)meshData.edges[index][0] + meshData.facePoints.Count + meshData.edgePoints.Count == (int)quadFace.z && index + meshData.facePoints.Count != (int)quadFace.y) ||
                        ((int)meshData.edges[index][1] + meshData.facePoints.Count + meshData.edgePoints.Count == (int)quadFace.z && index + meshData.facePoints.Count != (int)quadFace.y))
                    {
                        quadFace.w = index + meshData.facePoints.Count;
                        quadFace1.x = quadFace.x;
                        quadFace1.y = quadFace.w;
                        quadFace1.z = quadFace.z;
                        quadFace1.w = quadFace.y;

                        quadFaces.Add(quadFace1);
                        break;
                    }
                }
            }
        }
        return quadFaces;
    }

    private static List<Vector3> createQuadPoints(CCMeshData meshData)
    {
        List<Vector3> newQuadPoints = new List<Vector3>();
        foreach (var item in meshData.facePoints)
        {
            newQuadPoints.Add(item);
        }
        foreach (var item in meshData.edgePoints)
        {
            newQuadPoints.Add(item);
        }
        foreach (var item in meshData.newPoints)
        {
            newQuadPoints.Add(item);
        }
        return newQuadPoints;
    }

    // Returns a list of all edges in the mesh defined by given points and faces.
    // Each edge is represented by Vector4(p1, p2, f1, f2)
    // p1, p2 are the edge vertices
    // f1, f2 are faces incident to the edge. If the edge belongs to one face only, f2 is -1
    public static List<Vector4> GetEdges(CCMeshData mesh)
    {
        Vec2Comparer c = new Vec2Comparer();
        Dictionary<Vector2, Vector2> d = new Dictionary<Vector2, Vector2>(c);
        
        int counter = 0;
        Vector2 key = new Vector2(0, 0);
        Vector2 value = new Vector2(-1, -1);
        foreach (var face in mesh.faces)
        {
            for (int i = 0; i < 4; i ++)
            {
                key.x = face[i % 4];
                key.y = face[(i + 1) % 4];
                if (d.ContainsKey(key))
                {
                    value = d[key];
                    value.y = counter;
                    d[key] = value;
                }
                else
                {
                    value.x = counter;
                    value.y = -1;
                    d.Add(key, value);
                }
            }
            counter++;
        }

        List<Vector4> edges = new List<Vector4>();
        foreach (var item in d)
        {
            Vector4 vec4 = new Vector4(0,0,0, 0);
            vec4[0] = item.Key.x;
            vec4[1] = item.Key.y;
            vec4[2] = item.Value.x;
            vec4[3] = item.Value.y;
            edges.Add(vec4);
        }
        return edges;
    }

    // Returns a list of "face points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetFacePoints(CCMeshData mesh)
    {
        List<Vector3> facePoints = new List<Vector3>();
        foreach (var face in mesh.faces)
        {
            Vector3 facePoint = 0.25f * (mesh.points[(int) face[0]] + mesh.points[(int) face[1]] +
                                         mesh.points[(int) face[2]] + mesh.points[(int) face[3]]);
            facePoints.Add(facePoint);
        }
        return facePoints;
    }

    // Returns a list of "edge points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetEdgePoints(CCMeshData mesh)
    {
        List<Vector3> edgePoints = new List<Vector3>();
        foreach (var edge in mesh.edges)
        {
            Vector3 facePoint = 0.25f * (mesh.points[(int) edge[0]] + mesh.points[(int) edge[1]] +
                                         mesh.facePoints[(int) edge[2]] + mesh.facePoints[(int) edge[3]]);
            edgePoints.Add(facePoint);
        }
        return edgePoints;
    }

    // Returns a list of new locations of the original points for the given CCMeshData, as described in the CC algorithm 
    public static List<Vector3> GetNewPoints(CCMeshData mesh)
    {
        Dictionary<int, List<int>> n = createN(mesh);
        Dictionary<int, Vector3> f = createF(mesh, n);
        Dictionary<int, Vector3> r = createR(mesh);
        List<Vector3> newPoints = new List<Vector3>();
        Vector3 newPoint = new Vector3();
        for (int i = 0; i < mesh.points.Count; i++)
        {
            newPoint = (f[i] + 2 * r[i] + (n[i].Count - 3) * mesh.points[i]) / (float)n[i].Count;
            newPoints.Add(newPoint);
        }
        return newPoints;
    }

    private static Dictionary<int, Vector3> createR(CCMeshData mesh)
    {
        Dictionary<int, List<int>> temp = new Dictionary<int, List<int>>();
        for (int i = 0; i < mesh.edges.Count; i++)
        {
            if (temp.ContainsKey((int) mesh.edges[i][0]))
            {
                temp[(int) mesh.edges[i][0]].Add(i);
            }
            else
            {
                List<int> pointIndex = new List<int>();
                pointIndex.Add(i);
                temp.Add((int) mesh.edges[i][0], pointIndex);
            }
            if (temp.ContainsKey((int) mesh.edges[i][1]))
            {
                temp[(int) mesh.edges[i][1]].Add(i);
            }
            else
            {
                List<int> pointIndex = new List<int>();
                pointIndex.Add(i);
                temp.Add((int) mesh.edges[i][1], pointIndex);
            }
        }
        Dictionary<int, Vector3> r = new Dictionary<int, Vector3>();
        Vector3 averageEdge = new Vector3();
        Vector3 averageEdges = new Vector3();
        foreach (var item in temp)
        {
            averageEdges[0] = 0;
            averageEdges[1] = 0;
            averageEdges[2] = 0;

            foreach (var index in item.Value)
            {
                averageEdge[0] =0;
                averageEdge[1] =0;
                averageEdge[2] =0;

                averageEdge = 0.5f * (mesh.points[(int)mesh.edges[index][0]] + mesh.points[(int)mesh.edges[index][1]]);
                averageEdges += averageEdge;
            }
            r[item.Key] = averageEdges * 1f / item.Value.Count;
        }

        return r;
    }


    private static Dictionary<int, Vector3> createF(CCMeshData mesh, Dictionary<int, List<int>> n)
    {
        Dictionary<int, Vector3> f = new Dictionary<int, Vector3>();
        Vector3 average = new Vector3(0,0,0);
        foreach (var item in n)
        {
            average[0] =  0; 
            average[1] =  0;
            average[2] =  0; 
            foreach (var faceIndex in item.Value)
            {
                average += mesh.facePoints[faceIndex];
            }
            average *= 1f / item.Value.Count;
            f[item.Key] =  average;
        }
        return f;
    }


    private static Dictionary<int, List<int>> createN(CCMeshData mesh)
    {
        Dictionary<int, List<int>> n = new Dictionary<int, List<int>>();
        for (int i = 0; i < mesh.faces.Count; i++)
        {
            Vector4 vec = mesh.faces[i];
            for (int j = 0; j < 4; j++)
            {
                int pIndex = (int)vec[j];
                if (n.ContainsKey(pIndex))
                {
                    n[pIndex].Add(i);
                }
                else
                {
                    List<int> value = new List<int>();
                    value.Add(i);
                    n.Add(pIndex, value);
                }
            }
        }
        return n;

    }
}
