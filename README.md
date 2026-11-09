# Modélisation Géométrique
Avant le début de votre cours, je pensais que nous devions réaliser le tp seul. En me basant sur les solutions existantes pour réaliser des faces dans Unity je suis partie sur la création d'un mesh avec un MeshFilter.

## Fonctionnement

Ma Scene view est configuré en mode Wireframe et le script s'exécute directement en mode éditeur. Depuis ce dernier j'ai accès à différents paramètres relancer le calcul du mesh. Cela me permet d'accéder à différents paramètres et de relancer automatiquement le calcul du mesh lorsqu'ils sont modifiés.

Pour l'utiliser, il suffit d'attacher le script à un GameObject vide (Empty GameObject). Le mesh sera alors directement généré et pourra être modifié depuis les paramètres exposés dans l'inspecteur.

```c#
    [Delayed] public int m = 16;
    [Delayed] public float height = 1f;
    [Delayed] public float radius = 1f;
    [Delayed] public float truncatedHeight = 0f;
```

### Remarque
J'ai utilisé l'IA pour les parties concernant la gestion de plusieurs couleurs, lorsque je travaillais encore sans le mode Wireframe.

C'est notamment pour cette raison que certaines parties du code contiennent des éléments superflus, qui ne sont plus vraiment pertinents dans le fonctionnement actuel. On retrouve par exemple ce morceau de code :
```c#
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
```
