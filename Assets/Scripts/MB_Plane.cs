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
        Mesh = new Mesh
        {
            name = "GeneratedMesh",

            vertices = GenerateVerts(),
            triangles = GenerateTries(half)
        };
     
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter = gameObject.GetComponent<MeshFilter>();

        if (MeshFilter == null)
            MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
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
