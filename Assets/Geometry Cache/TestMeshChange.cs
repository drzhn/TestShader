using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMeshChange : MonoBehaviour
{
    int vertIndex = 0;

    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] uv = mesh.uv;

        mesh.Clear();

        bool[] isVertexOriginal = new bool[vertices.Length];
        for (int i = 0; i < isVertexOriginal.Length; i++)
        {
            isVertexOriginal[i] = true;
        }
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUv = new List<Vector2>();

        Debug.Log(vertices.Length);
        int count = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (isVertexOriginal[i])
            {
                newVertices.Add(vertices[i]);
                newUv.Add(uv[i]);

                for (int j = i + 1; j < vertices.Length; j++)
                {
                    if (vertices[i] == vertices[j])
                    {
                        isVertexOriginal[j] = false;
                        for (int k = 0; k < triangles.Length; k++)
                        {
                            if (triangles[k] == j)
                                triangles[k] = i;
                        }
                    }
                }
            }
        }
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = triangles;
        mesh.uv = newUv.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Debug.Log(vertIndex);
        //    Mesh mesh = GetComponent<MeshFilter>().mesh;
        //    Vector3[] vertices = mesh.vertices;
        //    Vector3[] normals = mesh.normals;
        //    vertices[vertIndex] += normals[vertIndex] * 20;
        //    mesh.vertices = vertices;
        //    //mesh.RecalculateNormals();
        //    //mesh.RecalculateBounds();
        //    vertIndex++;
        //}
    }
}
