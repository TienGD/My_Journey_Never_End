using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    private const int TileCount = 3;

    [Tooltip("0 = fixed to world, 1 = fixed to camera (no movement). Values between 0 and 1 create a parallax effect.")]
    [SerializeField] private float parallaxSpeed = 0f;

    private Transform cam;
    private Vector3 lastCamPos;
    private GameObject[] backgrounds;
    private float width;

    private void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("EndlessBackground requires a Main Camera in the scene.");
            return;
        }

        cam = Camera.main.transform;
        lastCamPos = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("EndlessBackground requires a SpriteRenderer.");
            return;
        }

        width = sr.bounds.size.x;
        backgrounds = new GameObject[TileCount];
        backgrounds[0] = gameObject;

        // Sinh 2 bản sao hai bên ngay ở frame đầu tiên
        backgrounds[1] = CreateClone(transform.position.x + width);
        backgrounds[2] = CreateClone(transform.position.x - width);
    }

    private GameObject CreateClone(float posX)
    {
        GameObject clone = Instantiate(gameObject, transform.parent);
        Destroy(clone.GetComponent<EndlessBackground>());
        clone.transform.position = new Vector3(posX, transform.position.y, transform.position.z);
        return clone;
    }

    private void LateUpdate()
    {
        if (cam == null || backgrounds == null) return;

        Vector3 deltaMovement = cam.position - lastCamPos;
        lastCamPos = cam.position;

        float parallaxOffsetX = deltaMovement.x * parallaxSpeed;
        float wrapSpan = width * TileCount;
        float halfSpan = wrapSpan * 0.5f;

        for (int i = 0; i < TileCount; i++)
        {
            Transform bgTransform = backgrounds[i].transform;
            bgTransform.position += new Vector3(parallaxOffsetX, 0f, 0f);

            while (cam.position.x > bgTransform.position.x + halfSpan)
            {
                bgTransform.position += new Vector3(wrapSpan, 0f, 0f);
            }

            while (cam.position.x < bgTransform.position.x - halfSpan)
            {
                bgTransform.position -= new Vector3(wrapSpan, 0f, 0f);
            }
        }
    }
}
