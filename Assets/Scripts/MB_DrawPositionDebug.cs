using UnityEngine;

// Créer des formes selon un tableau de position afin de les visualiser
public class MB_DrawPositionDebug : MonoBehaviour
{
    [SerializeField] GameObject _model = null;

    public void DebugVertices(Vector3[] positions)
    {
        var thisTransform = transform;
        for (int i = 0; i < positions.Length; i++)
        {
            var currentClone = Instantiate(_model, Vector3.zero, Quaternion.identity, thisTransform);
            currentClone.transform.localPosition = positions[i];
            currentClone.name = $"Clone_{i}";
        }
    }
}