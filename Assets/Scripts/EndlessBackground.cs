using System.Collections;
using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    [Tooltip("0 = fixed to world, 1 = fixed to camera (no movement). Values between 0 and 1 create a parallax effect.")]
    [SerializeField] private float parallaxSpeed = 0f;
    
    private Transform cam;
    private Vector3 lastCamPos;
    
    private GameObject[] backgrounds;
    private float width;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogError("EndlessBackground requires a SpriteRenderer."); return; }

        width = sr.bounds.size.x;
        backgrounds = new GameObject[3];
        backgrounds[0] = gameObject;

        // Sinh ra ngay lập tức ở frame đầu tiên
        backgrounds[1] = Instantiate(gameObject, transform.parent);
        Destroy(backgrounds[1].GetComponent<EndlessBackground>());
        backgrounds[1].transform.position = new Vector3(transform.position.x + width, transform.position.y, transform.position.z);

        backgrounds[2] = Instantiate(gameObject, transform.parent);
        Destroy(backgrounds[2].GetComponent<EndlessBackground>());
        backgrounds[2].transform.position = new Vector3(transform.position.x - width, transform.position.y, transform.position.z);
    }
    void LateUpdate()
    {
        if (cam == null) return;

        // Calculate how much the camera has moved
        Vector3 deltaMovement = cam.position - lastCamPos;
        lastCamPos = cam.position;

        // Move all backgrounds by the parallax amount
        for (int i = 0; i < 3; i++)
        {
            backgrounds[i].transform.position += new Vector3(deltaMovement.x * parallaxSpeed, 0, 0);
        }

        // Wrap backgrounds when they go out of camera view
        for (int i = 0; i < 3; i++)
        {
            // If camera moves too far right
            if (cam.position.x > backgrounds[i].transform.position.x + width * 1.5f)
            {
                backgrounds[i].transform.position += new Vector3(width * 3f, 0, 0);
            }
            // If camera moves too far left
            else if (cam.position.x < backgrounds[i].transform.position.x - width * 1.5f)
            {
                backgrounds[i].transform.position -= new Vector3(width * 3f, 0, 0);
            }
        }
    }
}
