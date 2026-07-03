using UnityEngine;

public class ShowSegmentWidth : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        if (sr != null)
        {
            float segmentWidth = sr.bounds.size.x;

            Debug.Log($"{gameObject.name} → largura: {segmentWidth}");
        }
        else
        {
            Debug.LogWarning($"Nenhum SpriteRenderer encontrado em {gameObject.name}");
        }
    }
}