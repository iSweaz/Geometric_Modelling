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

        Mesh = new Mesh
        {
            name = "GeneratedMeshSubdiv",

            vertices = GenerateVerts(size_x,size_y),
            triangles = GenerateTries()
        };
     
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();
        
        MeshFilter = gameObject.GetComponent<MeshFilter>();

        if (MeshFilter == null)
            MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
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


