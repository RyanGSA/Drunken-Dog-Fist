using UnityEngine;

public class BackgroundSegmentSetup : MonoBehaviour
{
    [SerializeField] private float targetWidth = 30f;

    void Start()
    {
        if (transform.childCount == 0)
        {
            return;
        }

        SpriteRenderer sr = transform.GetChild(0)
            .GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            return;
        }

        float currentWidth = sr.bounds.size.x;

        float scaleMultiplier = targetWidth / currentWidth;

        foreach (Transform child in transform)
        {
            Vector3 scale = child.localScale;

            child.localScale = new Vector3(
                scale.x * scaleMultiplier,
                scale.y * scaleMultiplier,
                scale.z
            );
        }

        currentWidth *= scaleMultiplier;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            child.localPosition = new Vector3(
                (i - 1) * currentWidth,
                0,
                0
            );
        }

    }
}