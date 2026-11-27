using UnityEngine;

public class MovingTrap : Trap
{
    [SerializeField] float speed;
    [SerializeField] float speedRotation;

    [SerializeField] Transform[] movingPoint;

    private int i;

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movingPoint[i].position, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, movingPoint[i].position) < 0.25f)
        {
            i++;

            if (i >= movingPoint.Length)
            {
                i = 0;
            }
        }

        if (transform.position.x > movingPoint[i].position.x)
        {
            transform.Rotate(new Vector3(0, 0, speedRotation * Time.deltaTime));
        }
        else
        {
            transform.Rotate(new Vector3(0, 0, -speedRotation * Time.deltaTime));
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
    
}
