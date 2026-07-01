using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;

    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;

    private Vector3 lastCameraPosition;
    private float startZ;
    private Transform[] segments;
    private float segmentWidth;
    private float totalWidth;

    void Start()
    {
        lastCameraPosition = cameraTransform.position;
        startZ = transform.position.z;

        int childCount = transform.childCount;
        segments = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
            segments[i] = transform.GetChild(i);

        // pega a largura real do sprite direto do primeiro segmento
        SpriteRenderer sr = segments[0].GetComponentInChildren<SpriteRenderer>();
        segmentWidth = sr.bounds.size.x;
        totalWidth = segmentWidth * segments.Length;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        transform.position += new Vector3(
            deltaMovement.x * parallaxFactor,
            deltaMovement.y * parallaxFactor,
            0
        );

        lastCameraPosition = cameraTransform.position;
        transform.position = new Vector3(transform.position.x, transform.position.y, startZ);

        // reciclagem infinita
        float camX = cameraTransform.position.x;

        foreach (Transform seg in segments)
        {
            float distFromCam = camX - seg.position.x;

            if (Mathf.Abs(distFromCam) >= totalWidth / 2f)
            {
                float direction = Mathf.Sign(distFromCam);
                seg.position += new Vector3(totalWidth * direction, 0, 0);
            }
        }
    }
}