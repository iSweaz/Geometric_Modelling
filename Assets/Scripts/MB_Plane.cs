using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MB_Plane : MonoBehaviour
{

    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    [SerializeField] private bool _half = false;

    void Awake () 
    {
       CalculateMesh(_half);
    }

    void OnValidate()
    {
        CalculateMesh(_half);
    }


    void CalculateMesh(bool half)
    {
        Vector3[] vertices = GenerateVerts();
        int[] triangles = GenerateTries(half);

        int triangleCount = triangles.Length / 3;

        Mesh = new Mesh
        {
            name = "GeneratedMesh",
            vertices = vertices,
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

        // Un matériau par sous-mesh
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material[] materials = new Material[triangleCount];

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        for (int i = 0; i < triangleCount; i++)
        {
            materials[i] = new Material(shader);
            materials[i].color = Random.ColorHSV();
        }

        meshRenderer.sharedMaterials = materials;
    }


    Vector3[] GenerateVerts()
    {
        return new Vector3[]
        {
            //right side
            new Vector3(-1.5f,0,0),
            new Vector3(-1.5f,2,0),
            new Vector3(1.5f,2,0),
            new Vector3(1.5f,0,0)
   
        };
    }

    int[] GenerateTries(bool half)
    {
        if (half)
            return new int[]
            {
                //start with 1 for the normal orientation
                0,1,2
            };

        return new int[]
        {
            0,1,2,
            0,2,3
        };
    }

}
