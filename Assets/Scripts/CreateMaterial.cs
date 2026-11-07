using UnityEngine;

public  class CreateMaterial
{
    public static Material AddMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        return new Material(shader) {color = color};
    }

    public static Material[] randomsMaterials(int triesCount)
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
                color = Random.ColorHSV()
            };
        }
        return materials;
    }
}