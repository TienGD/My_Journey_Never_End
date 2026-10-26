using UnityEngine;

public class Parralax : MonoBehaviour
{
    private GameObject cam;
    [SerializeField] private float parallaxEffect;

    private float length;
    private float xPositon;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GameObject.Find("Main Camera");
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        xPositon = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceMoved = cam.transform.position.x * (1 - parallaxEffect);
        float distanceToMove = cam.transform.position.x * parallaxEffect;
        transform.position = new Vector3(xPositon + distanceToMove, transform.position.y);

        if (distanceMoved > xPositon + length)
        {
            xPositon = xPositon + length;
        }
        else if (distanceMoved < xPositon - length)
        {
            xPositon = xPositon - length;
        }

        
    }
}
