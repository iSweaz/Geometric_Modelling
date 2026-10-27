using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MB_Sphere : MonoBehaviour
{
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

                    public float radius;
    [Range(1,10)]   public int m;
    [Range(2,100)]  public int parallels;
    public Vector3 CENTER = Vector3.zero;
    

    void Awake()
    {
        
    }

    void GenerateMesh()
    {
        Mesh  = new Mesh()
        {
            
        };

        MeshFilter = GetComponent<MeshFilter>();
        if(MeshFilter)
            MeshFilter.sharedMesh = Mesh;    
    }


}
