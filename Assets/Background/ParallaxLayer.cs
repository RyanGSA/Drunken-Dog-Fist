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

        SpriteRenderer sr = segments[0].GetComponentInChildren<SpriteRenderer>();
        segmentWidth = sr.bounds.size.x;
        totalWidth = segmentWidth * segments.Length;

        PositionSegments();
    }

    void PositionSegments()
    {
        // Ordena os segmentos pela posição X atual, assim identificamos
        // automaticamente quem é o da esquerda, o do meio e o da direita,
        // não importa a ordem dos filhos na hierarquia.
        System.Array.Sort(segments, (a, b) => a.position.x.CompareTo(b.position.x));

        // Assume 3 segmentos (esquerda, centro, direita).
        Transform left = segments[0];
        Transform center = segments[1];
        Transform right = segments[2];

        // Reposiciona os laterais com base no X atual do central.
        left.position = new Vector3(
            center.position.x - segmentWidth,
            left.position.y,
            left.position.z
        );

        right.position = new Vector3(
            center.position.x + segmentWidth,
            right.position.y,
            right.position.z
        );
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