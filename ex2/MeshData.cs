using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


public class MeshData
{
    public List<Vector3> vertices; // The vertices of the mesh 
    public List<int> triangles; // Indices of vertices that make up the mesh faces
    public Vector3[] normals; // The normals of the mesh, one per vertex

    // Class initializer
    public MeshData()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }
    

    // Returns a Unity Mesh of this MeshData that can be rendered
    public Mesh ToUnityMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals
        };

        return mesh;
    }

    // Calculates surface normals for each vertex, according to face orientation
    public void CalculateNormals()
    {
        normals = new Vector3[vertices.Count];
        List<Vector3> faceNormals = Faces(); // normalized normal of all faces
        for (int i = 0; i < vertices.Count; i++)
        {
            List<int> triangleIndex = TrianglesWithVertices(i); // get list of triangles that use i vertices
            NormaliseVertices(triangleIndex, faceNormals, i);
        }
    }

    // Edits mesh such that each face has a unique set of 3 vertices
    public void MakeFlatShaded()
    {
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i++)
        {
            newVertices.Add(vertices[triangles[i]]);
            newTriangles.Add(i);
        }
        triangles = newTriangles;
        vertices = newVertices;
        //CalculateNormals();


        // Your implementation
    }

    private List<Vector3> Faces()
    {
        List<Vector3> facesList = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector3 first = vertices[triangles[i]] - vertices[triangles[i + 2]];
            Vector3 second = vertices[triangles[i + 1]] - vertices[triangles[i + 2]];
            facesList.Add(Vector3.Normalize(Vector3.Cross(first, second)));
        }
        return facesList;
    }

    private List<int> TrianglesWithVertices(int verticesNum)
    {
        List<int> triangleIndex = new List<int>();
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i] == verticesNum)
            {
                triangleIndex.Add(i/3);
            }
        }
        return triangleIndex;
    }

    private void NormaliseVertices(List<int> triangleIndex, List<Vector3> faceNormals, int verticesNum)
    {
        Vector3 sum = Vector3.zero;
        foreach (var item in triangleIndex)
        {
            sum += faceNormals[item];
        }
        normals[verticesNum] = Vector3.Normalize(sum);
    }
    
}