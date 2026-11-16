using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
[ExecuteInEditMode]
public class MB_Loader : MonoBehaviour
{
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [Delayed]
    public string fileName;
    public Color color;
    
    void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        MeshOff meshOff = new MeshOff();
        if(fileName =="")
            return;
        meshOff.LoadOffMesh(fileName, out _mesh);
        _meshFilter.sharedMesh = _mesh;

        Material mat = CreateMaterial.AddMaterial(color);
        mat.SetFloat("_WorkflowMode", 0f);
        mat.SetFloat("_Smoothness", 0f);
        _meshRenderer.sharedMaterials = new Material[] { mat };
    }
}