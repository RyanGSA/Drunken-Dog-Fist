using UnityEngine;

public class FitSegmentWidth : MonoBehaviour
{
    [SerializeField] private float targetWidth = 30f;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            return;
        }

        float currentWidth = sr.bounds.size.x;

        float scaleMultiplier = targetWidth / currentWidth;

        Vector3 scale = transform.localScale;

        transform.localScale = new Vector3(
            scale.x * scaleMultiplier,
            scale.y * scaleMultiplier,
            scale.z
        );

    }
}