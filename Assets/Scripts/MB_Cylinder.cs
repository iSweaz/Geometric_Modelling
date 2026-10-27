using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MB_Cylinder : MonoBehaviour
{
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    [Delayed] public float height = 1.0f;
    /// <summary>
    /// Limité entre 3 et 64
    /// </summary>
    [Delayed] public int m = 16;
    [Delayed] public float radius = 2.0f;

    public bool colored = false;

    struct Plane
    {
       public Vector3 coord;

        public Plane (float h, float r, double theta)
        {
            coord.x = (float)(r * Math.Cos(theta));
            coord.y = (float)(r * Math.Sin(theta));
            coord.z = h/2;
        }

        public override string ToString() => $"({coord.x:F2}, {coord.y:F2}, {coord.z:F2})";
    }


    void Awake()
    {
        CalculateMesh();
    }

    void CalculateMesh(bool Colored = false)
    {          
        m = Math.Clamp(m, 3, 64);
        if (Colored)
        {
            int[] triangles = GenerateTries();
            int triangleCount = triangles.Length / 3;

            Mesh = new Mesh
            {
                name = "GeneratedMesh",
                vertices = GenerateVerts(),
                subMeshCount = triangleCount
            };


            // Un sous-mesh par triangle
            for (int i = 0; i < triangleCount; i++)
            {
                Mesh.SetTriangles(new int[]
                {
                    triangles[i * 3],
                    triangles[i * 3 + 1],
                    triangles[i * 3 + 2]
                }, i);
            }

            Mesh.RecalculateNormals();
            Mesh.RecalculateBounds();

            MeshFilter = GetComponent<MeshFilter>();

            if (MeshFilter == null)
                MeshFilter = gameObject.AddComponent<MeshFilter>();
            MeshFilter.sharedMesh = Mesh;

            MeshRenderer meshRendererColored = GetComponent<MeshRenderer>();
            meshRendererColored.sharedMaterials = randomsMaterials(triangleCount);
            return;
        }

        Mesh = new Mesh
        {
            name = "GeneratedMesh",
            vertices = GenerateVerts(),
            triangles =  GenerateTries()
        };

        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter = GetComponent<MeshFilter>();
        if (MeshFilter == null)
            MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.sharedMesh = Mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material mat = CreateMaterial(Color.cyan);
        mat.SetFloat("_WorkflowMode", 0f);
        mat.SetFloat("_Smoothness", 0f);
        meshRenderer.sharedMaterials = new Material[] { mat };
    }


    Vector3[] GenerateVerts()
    {
        Vector3[] vertices = new Vector3[2 * m + 2];

        for (int i = 0; i < m; i++)
        {
            double theta = 2 * Math.PI * i / m;
            vertices[i]     = new Plane(-height, radius, theta).coord;
            vertices[i + m] = new Plane( height, radius, theta).coord;
        }

        vertices[2 * m]     = new Vector3(0, 0, -height / 2); // centre bas
        vertices[2 * m + 1] = new Vector3(0, 0,  height / 2); // centre haut

        return vertices;
    }

    int[] GenerateTries()
    {
        const int IndicesParTranche = 12; // 4 triangles × 3 indices
        int[] triangles = new int[m * 12];
        int centerBot = 2 * m;
        int centerTop = 2 * m + 1;

        for (int i = 0; i < m; i++)
        {
            int next = (i + 1) % m;

            int b0 = i,     b1 = next;
            int t0 = i + m, t1 = next + m;

            // Début du bloc de 12 indices réservé à la tranche i
            // (4 triangles × 3 indices : 2 pour la paroi, 1 par bouchon)
            int k = i * IndicesParTranche; 

            triangles[k]     = b0; triangles[k + 1]  = b1; triangles[k + 2]  = t0;
            triangles[k + 3] = b1; triangles[k + 4]  = t1; triangles[k + 5]  = t0;

            triangles[k + 6] = centerBot; triangles[k + 7] = b1; triangles[k + 8] = b0;

            triangles[k + 9] = centerTop; triangles[k + 10] = t0; triangles[k + 11] = t1;
        }

        return triangles;
    }

    private void OnValidate()
    {
        CalculateMesh(colored);
    }

    Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        return new Material(shader) { color = color};
    }

    Material[] randomsMaterials(int triesCount)
    {
        // Un matériau par sous-mesh
        Material[] materials = new Material[triesCount];

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        for (int i = 0; i < triesCount; i++)
        {
            materials[i] = new Material(shader)
            {
                color = UnityEngine.Random.ColorHSV()
            };
        }
        return materials;
    }

}
