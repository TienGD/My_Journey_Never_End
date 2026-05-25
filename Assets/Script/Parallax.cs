using UnityEngine;

public class Parralax : MonoBehaviour
{
    [SerializeField] float depth = 1f;
    Player player;
    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        float realVelocity = player.velocity.x / depth;

        Vector2 pos = transform.position;

        pos.x -= realVelocity * Time.fixedDeltaTime;

        if( pos.x <= -50)
        {
            pos.x = 50;
        }

        transform.position = pos;
    }
}
