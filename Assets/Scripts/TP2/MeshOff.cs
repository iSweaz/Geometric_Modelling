using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class MeshOff
{   
    public class Edge
    {
        public Vector3 edge;
        public Edge(Vector3 p1, Vector3 p2)
        {
            edge = p2 - p1;
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

        // string[] vectorsText = new string[sizeVertices]; 
        // Array.Copy(elements, 2, vectorsText, 0, sizeVertices);
        // other version with exclusive input at the end : string[] vectors = elements[2..(sizeVertices + 2)]; 

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

        Triangles[] triangles = new Triangles[sizeFaces];
        for (int v = sizeVertices + 2, i = 0; 
            v < elements.Length; 
            v++, i++)
        {
            string[] index = elements[v].Split(" ");
            
            triangles[i] = new Triangles(
                int.Parse(index[1]),
                int.Parse(index[2]),
                int.Parse(index[3])
            );
        }

        int[] indexTriangles = triangles
            .SelectMany(t => t.IntArray())
            .ToArray();

        return new Mesh
        {
            name = "GeneratedMesh",
            vertices = vertices,
            triangles = indexTriangles,
            normals = CalculateNormalWithVertices(vertices)
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

}
