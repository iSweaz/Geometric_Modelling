using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

    public class Face
    {
        public int[] face;
        public Face(int f1, int f2, int f3)
        {
            face = new int[3];
            face[0] = f1;
            face[1] = f2;
            face[2] = f3;
        }
    }

    public struct MeshStruct
    {   
        public Vector3[] coordinates;
        public Edge[] edges;
        public Face[] faces;
        public MeshStruct(Vector3[] coordinates,Edge[] edges, Face[] faces)
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
        }

        return new Mesh
        {
            name = "GeneratedMesh",
            vertices = vertices,
            triangles = triangles
        };
    }

    Vector3[] CenterMesh(Vector3[] vertices)
    {
        Vector3 CG =  Vector3.zero;
        Array.ForEach(vertices, i => CG += i);
        CG /= vertices.Length;

        for(int i = 0; i < vertices.Length; i++)
            vertices[i] -= CG; 
        
        return vertices;
    }
}
