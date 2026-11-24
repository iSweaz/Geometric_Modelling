using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class MeshOff
{   
    private string[] verticesData, trianglesData, normalsData;
    public class Edge
    {
        public Vector3 P1 { get; private set; }
        public Vector3 P2 { get; private set; }

        public Vector3 Direction => P2 - P1;
        public float Length => Direction.magnitude;

        public Edge(Vector3 p1, Vector3 p2)
        {
            P1 = p1;
            P2 = p2;
        }
    }

    public class Triangles
    {
        public int p1, p2, p3;
        public Vector3 v1, v2, v3;

        public Triangles(int p1, int p2, int p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public Triangles(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public int[] IntArray() => new int[]{p1,p2,p3};
        public Vector3[] VecArray() => new Vector3[]{v1,v2,v3};
    }

    public class MeshData
    {   
        public Vector3[] Vertices { get; private set; }
        public Edge[] Edges { get; private set; }
        public Triangles[] Triangles { get; private set; }

        public MeshData(Vector3[] vertices, Edge[] edges, Triangles[] triangles)
        {
            Vertices = vertices;
            Edges = edges;
            Triangles = triangles;
        }

        public MeshData(Mesh mesh)
        {
            Vertices = mesh.vertices;
            Edges = Array.Empty<Edge>();
            Triangles = SetTriangles(mesh.triangles);
        }

        public void SetVertices(Vector3[] vertices)
        {
            Vertices = vertices;
        }

        public void SetEdges(Edge[] edges)
        {
            Edges = edges;
        }

        public Triangles[] SetTriangles(int[] indices)
        {
            if (indices.Length % 3 != 0)
                throw new ArgumentException("The number of indices must be divisible by 3.");

            Triangles[] result = new Triangles[indices.Length/3];
            int i = 0;
            while (i < indices.Length)
            {
                result[i] = new Triangles(
                    indices[i],
                    indices[i+1],
                    indices[i+2]
                    ); 
                i += 3;
            }
            return result;
        }
    }


    public void LoadOffMesh(string fileName, out Mesh mesh)
    {
        string[] elements = ReadFile(fileName);
        
 
        mesh = CalculateMesh(elements);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        string[] newTriangles = trianglesData, newVertices = verticesData;
        damageMesh(ref newTriangles, ref newVertices);
        WriteFile(fileName, newVertices, newTriangles);
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

    string[] WriteFile(string fileName, string[] verticesData, string[] trianglesData)
    {
        int verticeSize     = verticesData.Length;
        int trianglesSize   = trianglesData.Length;
        int normalsSize     = normalsData.Length;

            

        string path = Application.dataPath + "/Mesh_off/" +  "edited_" + fileName;
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(@$"OFF
{verticeSize} {trianglesSize} 0 {normalsSize}");
            foreach(string vert in verticesData)
                sw.WriteLine(vert);
            foreach(string trig in trianglesData)
                sw.WriteLine(trig);
            foreach(string normal in normalsData)
                sw.WriteLine(normal);
        }
        return null;
    }

    Mesh CalculateMesh(string[] elements)
    {
        string[] MeshInfo = elements[1].Split(" ");      

        int sizeVertices = int.Parse(MeshInfo[0]);
        int sizeFaces = int.Parse(MeshInfo[1]);

        // string[] vectorsText = new string[sizeVertices]; 
        // Array.Copy(elements, 2, vectorsText, 0, sizeVertices);
        // other version with exclusive input at the end : string[] vectors = elements[2..(sizeVertices + 2)]; 

        Vector3[] vertices = new Vector3[sizeVertices];
        verticesData = new string[sizeVertices];
        for(int v = 0; v < sizeVertices; v++)
        {
            verticesData[v] = elements[v + 2];
            string []vector  = verticesData[v].Split(" ");
            vertices[v] = new Vector3(
                float.Parse(vector[0], CultureInfo.InvariantCulture),
                float.Parse(vector[1], CultureInfo.InvariantCulture),
                float.Parse(vector[2], CultureInfo.InvariantCulture));
        }

        vertices = CenterMesh(vertices);

        trianglesData = new string[sizeFaces];
        Triangles[] triangles = new Triangles[sizeFaces];
        for (int v = sizeVertices + 2, i = 0; 
            v < elements.Length; 
            v++, i++)
        {
            trianglesData[i] = elements[v];
            string[] index = trianglesData[i].Split(" ");
            
            triangles[i] = new Triangles(
                int.Parse(index[1]),
                int.Parse(index[2]),
                int.Parse(index[3])
            );
        }

        int[] indexTriangles = triangles
            .SelectMany(t => t.IntArray())
            .ToArray();
        
        Vector3[] normals = CalculateNormalWithVertices(vertices);
        normalsData = new string[normals.Length];
        for(int i = 0; i < normals.Length; i++)
        {
            normalsData[i] = vertices[i].x + " " + vertices[i].y + " " + vertices[i].z;
        }            

        return new Mesh
        {
            name = "GeneratedMesh",
            vertices = vertices,
            triangles = indexTriangles,
            normals = normals
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
    Vector3[] CalculateNormalWithTriangles(Triangles[] triangles)
    {
        Vector3[] normals = new Vector3[triangles.Length];
        for(int i = 0; i < triangles.Length; i++)
        {
            Triangles trig = triangles[i];
            Vector3  u = trig.v2 - trig.v1;
            Vector3  v = trig.v3 - trig.v1;

            normals[i] = new Vector3(
                u.y * v.z - u.z * v.y,
                u.z * v.x - u.x * v.z,
                u.x * v.y - u.y * v.x
            );
        }
        return normals;
    }

    //https://wikis.khronos.org/opengl/Calculating_a_Surface_Normal   Newell's Method
    Vector3[] CalculateNormalWithVertices(Vector3[] vertices)
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

    void damageMesh(ref string[] triangles, ref string[] vertices, int n = 1500)
    {
        n = Mathf.Clamp(n, 0, triangles.Length);

        List<string> remainingTriangles = new List<string>(triangles);
        
        for (int i = 0; i < n; i++)
        {
            int index = UnityEngine.Random.Range(0, remainingTriangles.Count);
            remainingTriangles.RemoveAt(index);
        }

        //Optimized for searching, HashSet is a List with no duplicate members. Can't use indices with a HashSet
        //unordered_set <T> for cpp
        HashSet<int> usedVertices = new HashSet<int>();

        foreach (string triangle in remainingTriangles)
        {
            string[] indices = triangle.Split(" ");

            for (int i = 1; i <= 3; i++)
                usedVertices.Add(int.Parse(indices[i]));
        }

        int[] indexMapping = new int[vertices.Length];
        List<string> newVertices = new List<string>();

        for (int i = 0; i < vertices.Length; i++)
        {
            indexMapping[i] = -1;

            if (usedVertices.Contains(i))
            {
                indexMapping[i] = newVertices.Count;
                newVertices.Add(vertices[i]);
            }
        }

        List<string> newTriangles = new List<string>();

        foreach (string triangle in remainingTriangles)
        {
            string[] indices = triangle.Split(" ");

            for (int i = 1; i <= 3; i++)
            {
                int oldIndex = int.Parse(indices[i]);
                indices[i] = indexMapping[oldIndex].ToString();
            }

            newTriangles.Add(string.Join(" ", indices));
        }

        vertices = newVertices.ToArray();
        triangles = newTriangles.ToArray();
    }
}
