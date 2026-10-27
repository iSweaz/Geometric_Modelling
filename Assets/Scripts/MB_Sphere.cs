using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MB_Sphere : MonoBehaviour
{
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    [Delayed]   public float radius = 1;
    [Delayed]   public int m = 3; //divisions verticales de la sphère.
    [Delayed]   public int p = 2 ; //divisions horizontales de la sphère.
    private Vector3 CENTER = Vector3.zero, NORTH, SOUTH;
    
    struct Sphere
    {
       public Vector3 coord;

        public Sphere (float r, float theta, double phi)
        {
            coord.x = r * (float)(Math.Sin(theta)) * (float)(Math.Cos(phi));
            coord.y = r * (float)(Math.Cos(theta));
            coord.z = r * (float)(Math.Sin(theta)) * (float)(Math.Sin(phi));
        }

        public override string ToString() => $"({coord.x:F2}, {coord.y:F2}, {coord.z:F2})";
    }

    void Awake()
    {
        GenerateMesh();
    }

    void OnValidate()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        m = Mathf.Max(3, m);
        p = Mathf.Max(2, p);
        NORTH = new Vector3(0f,radius,0f);
        SOUTH = new Vector3(0f,-radius,0f);
        Mesh  = new Mesh()
        {
            vertices = Vertices(),
            triangles = Triangles()
        };


        
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter = GetComponent<MeshFilter>();
        if (MeshFilter == null)
            MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.sharedMesh = Mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial =  CreateMaterial(Color.yellow);
    }

    Vector3[] Vertices()
    {
        Vector3[] vertices = new Vector3[m * p + 2];
        vertices[0] = NORTH;
        int index = 1;
        for (int i = 1; i <= m; i++)
        {
            float theta = Mathf.PI * i / (m + 1);

            for (int j = 0; j < p; j++)
            {
                float phi = 2 * Mathf.PI * j / p + Mathf.PI / 2;
                vertices[index] = new Sphere(radius, theta, phi).coord;

                index++;
            }
        }
        vertices[index] = SOUTH;

        return vertices;
    }

    int[] Triangles()
    {
        int[] triangles = new int[6 * m * p];
        int k = 0; // prochaine case libre du tableau

        for (int j = 0; j < p; j++)
        {
            int next = (j + 1) % p;
            triangles[k++] = 1 + j;
            triangles[k++] = 0;
            triangles[k++] = 1 + next;
        }

        for (int i = 0; i < m - 1; i++)
        {
            for (int j = 0; j < p; j++)
            {
                int next = (j + 1) % p;

                int b0 = 1 + i * p + j;          // anneau du haut, position j
                int b1 = 1 + i * p + next;       // anneau du haut, position suivante
                int t0 = 1 + (i + 1) * p + j;    // anneau du bas, position j
                int t1 = 1 + (i + 1) * p + next; // anneau du bas, position suivante

               // 1) Bandes
                triangles[k++] = b0; triangles[k++] = b1; triangles[k++] = t0;
                triangles[k++] = b1; triangles[k++] = t1; triangles[k++] = t0;
            }
        }

        int dernierAnneau = 1 + (m - 1) * p;
        int sud = m * p + 1;
        for (int j = 0; j < p; j++)
        {
            int next = (j + 1) % p;
            triangles[k++] = dernierAnneau + j;
            triangles[k++] = dernierAnneau + next;
            triangles[k++] = sud;
        }

        return triangles;
    }

    Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        return new Material(shader) { color = color};
    }
}
