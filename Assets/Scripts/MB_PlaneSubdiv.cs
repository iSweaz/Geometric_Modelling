using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MB_PlaneSubdiv : MonoBehaviour
{

    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    public int lignes = 4, colonnes = 4;

    public float width = 3, height = 2;

    void Awake () 
    {
        CalculateSubdivMesh();
    }

    void OnValidate()
    {
        CalculateSubdivMesh();
    }

    void CalculateSubdivMesh()
    {
        float size_x = width / colonnes;
        float size_y = height / lignes;

         Vector3[] vertices = GenerateVerts(size_x,size_y);
        int[] triangles = GenerateTries();

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
        
        MeshFilter = gameObject.GetComponent<MeshFilter>();

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

    /// <summary>
    /// Calcul la position des points par rapport aux dimensions souhaités et le nombres de lignes et de colonnes attendues. 
    /// </summary>
    /// <param name="size_x">Distance entre chaque point horizontal en fonction de width</param>
    /// <param name="size_y">Distance entre chaque point vertical en fonction de height</param>
    /// <returns></returns>
    Vector3[] GenerateVerts(float size_x, float size_y)
    {
        List<Vector3> array = new List<Vector3>();
       float y = height/2;

        for(int i = 0; i <= lignes; ++i)
        {
            float x = -width/2;
            for(int j = 0; j <= colonnes; ++j)
            {
                array.Add(new Vector3(x,y,0));
                x += size_x;
            } 
            y -= size_y;
        }
        return array.ToArray();
    }

    /// <summary>
    /// Définit l'ordre des points pour chaque quads.
    /// </summary>
    /// <returns>Retourne un tableau de points pour dessiner les triangles.</returns>
    int[] GenerateTries()
    {
        int m = colonnes + 1; // sommets par ligne
        List<int> triangles = new List<int>(lignes * colonnes * 6);

        for (int i = 0; i < lignes; i++)
        {
            for (int j = 0; j < colonnes; j++)
            {
                int a = i * m + j; // haut-gauche
                int b = a + 1;     // haut-droite
                int c = a + m;     // bas-gauche
                int d = c + 1;     // bas-droite

                // triangle 1
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                // triangle 2
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
            }
        }

        return triangles.ToArray();
    }

}


