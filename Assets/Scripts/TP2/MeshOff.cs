using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshOff
{   
    public class Edge
    {
        private Vector3[] edge;
        public Edge(Vector3 vector1, Vector3 vector2)
        {
            edge = new Vector3[2];
            edge[0] = vector1;
            edge[1] = vector2;
        }
    }

    public class Triangles
    {
        public Vector3 p1,p2,p3;
        public Triangles(Vector3 p1, Vector3 p2, Vector3 p3)
        {            
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }

    public struct MeshStruct
    {   
        public Vector3[] coordinates;
        public Edge[] edges;
        public Triangles[] faces;
        public MeshStruct(Vector3[] coordinates,Edge[] edges, Triangles[] faces)
        {
            this.coordinates = coordinates;
            this.edges = edges;
            this.faces = faces;
        }
    }


    public void LoadOffMesh(string fileName, out Mesh mesh)
    {
        string[] elements = ReadFile(fileName);
        
 
        mesh = CalculateMesh(elements);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    string[] ReadFile(string fileName)
    {
        string path = Application.dataPath + "/Mesh_off/" +  fileName;
        if (!File.Exists(path)) 
        {
            Debug.LogError("File not found.");
            return null;
        }

        using (StreamReader sr = new StreamReader(path))
        {
            var fileContents = sr.ReadToEnd();
            return fileContents.Split(
                new char[] { '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries
            );
        }
    }

    Mesh CalculateMesh(string[] elements)
    {
        string[] MeshInfo = elements[1].Split(" ");      

        int sizeVertices = int.Parse(MeshInfo[0]);
        int sizeFaces = int.Parse(MeshInfo[1]);

        // string[] vectorsText = new string[sizeVertices]; // other version with exclusive input at the end, + use more memory : string[] vectors = elements[2..(sizeVertices + 2)]; 
        // Array.Copy(elements, 2, vectorsText, 0, sizeVertices);

        Vector3[] vertices = new Vector3[sizeVertices];
        for(int v = 0; v < sizeVertices; v++)
        {
            string []vector  = elements[v + 2].Split(" ");
            vertices[v] = new Vector3(
                float.Parse(vector[0], CultureInfo.InvariantCulture),
                float.Parse(vector[1], CultureInfo.InvariantCulture),
                float.Parse(vector[2], CultureInfo.InvariantCulture));
        }

        vertices = CenterMesh(vertices);

        Triangles[] trig = new Triangles[sizeFaces];
        int[] triangles = new int[sizeFaces * 3];
        for (int v = sizeVertices + 2, i = 0; 
            v < elements.Length; 
            v++, i++)
        {
            string[] index = elements[v].Split(" ");
            int k = i * 3;
            triangles[k] = int.Parse(index[1]);
            triangles[k + 1] = int.Parse(index[2]);
            triangles[k + 2] = int.Parse(index[3]);
            trig[i] = new Triangles(
                vertices[int.Parse(index[1])],
                vertices[int.Parse(index[2])],
                vertices[int.Parse(index[3])]
            );
        }

        

        return new Mesh
        {
            name = "GeneratedMesh",
            vertices = vertices,
            triangles = triangles,
            normals = CalculateNormal(vertices)
        };
    }

    Vector3[] CenterMesh(Vector3[] vertices)
    {
        Vector3 CG =  Vector3.zero;
        Array.ForEach(vertices, i => CG += i);
        CG /= vertices.Length;

        for(int i = 0; i < vertices.Length; i++)
            vertices[i] -= CG; 
        
        return NormalizeMesh(vertices);
    }

    Vector3[] NormalizeMesh(Vector3[] vertices)
    {
        float max = 0f;
        foreach (Vector3 vertex in vertices)
        {
            max = Mathf.Max(max,
                Mathf.Abs(vertex.x),
                Mathf.Abs(vertex.y),
                Mathf.Abs(vertex.z));
        }

        if (max > 0f)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] /= max;
            }
        }
        return vertices;
    }

    //https://wikis.khronos.org/opengl/Calculating_a_Surface_Normal
    void CalculateNormal(out Vector3[] normals, Triangles[] triangles)
    {
        normals = new Vector3[triangles.Length];
        for(int i = 0; i < triangles.Length; i++)
        {
            Triangles trig = triangles[i];
            Vector3  u = trig.p2 - trig.p1;
            Vector3  v = trig.p3 - trig.p1;

            normals[i] = new Vector3(
                u.y * v.z - u.z * v.y,
                u.z * v.x - u.x * v.z,
                u.x * v.y - u.y * v.x
            );
        }
    }

    //https://wikis.khronos.org/opengl/Calculating_a_Surface_Normal   Newell's Method
    Vector3[] CalculateNormal(Vector3[] vertices)
    {
        Vector3[] normals = new Vector3[vertices.Length];
        for(int i = 0; i < vertices.Length; i++)
        {
            Vector3 current = vertices[i];
            Vector3 next = vertices[(i+1) % vertices.Length];

            normals[i] = new Vector3(
                    (current.y - next.y) * (current.z - next.z),
                    (current.z - next.z) * (current.x - next.x),
                    (current.x - next.x) * (current.y - next.y)
                );
        }
        return normals;
    }

}
