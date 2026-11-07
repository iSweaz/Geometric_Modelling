using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MB_Cone : MonoBehaviour
{
    struct Circle
    {
       public Vector3 coord;

        public Circle (float h, float r, double theta)
        {
            coord.x = (float)(r * Math.Cos(theta));
            coord.y = (float)(r * Math.Sin(theta));
            coord.z = h;
        }

        public override string ToString() => $"({coord.x:F2}, {coord.y:F2}, {coord.z:F2})";
    }

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    
    [Delayed] public int m = 16;
    [Delayed] public float height = 1f;
    [Delayed] public float radius = 1f;
    [Delayed] public float truncatedHeight = 0f;
    

    void CalculateMesh()
    {
        ClampValues();
        _mesh = new Mesh
        {
            name = "GeneratedMesh",
            vertices = GenerateVerts(),
            triangles =  GenerateTries()
        };

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.sharedMesh = _mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material mat = CreateMaterial.AddMaterial(Color.coral);
        mat.SetFloat("_WorkflowMode", 0f);
        mat.SetFloat("_Smoothness", 0f);
        meshRenderer.sharedMaterials = new Material[] { mat };
    }

    Vector3[] GenerateVerts()
    {
        Vector3[] vertices = new Vector3[m + 2];

        for (int i = 0; i < m; i++)
        {
            double theta = 2 * Math.PI * i / m;
            vertices[i]  = new Circle(0, radius, theta).coord;
        }

        vertices[m]     = Vector3.zero; 
        vertices[m + 1] = new Vector3(
            0,
            0,
            truncatedHeight <= 0 ? height : truncatedHeight
        );
        Debug.Log( vertices[m + 1]);
        return vertices;
    }

    int[] GenerateTries()
    {
        const int IndicesParTranche = 6; // 2 triangles × 3 indices
        int[] triangles = new int[m * IndicesParTranche];
        int centerBot = m;
        int centerTop = m + 1;

        for (int i = 0; i < m; i++)
        {
            int next = (i + 1) % m;

            int b0 = i,     b1 = next;

            int k = i * IndicesParTranche; 

            triangles[k]     = b1; triangles[k + 1] = b0; triangles[k + 2] = centerBot;

            triangles[k + 3] = b0; triangles[k + 4] = b1; triangles[k + 5] = centerTop;

        }

        return triangles;
    }

    void OnValidate()
    {
        CalculateMesh();
    }

    void Awake()
    {
        CalculateMesh();
    }

    void ClampValues()
    {
        Mathf.Clamp(truncatedHeight,0, height);
    }
}
