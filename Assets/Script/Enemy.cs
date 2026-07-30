using UnityEngine;

public class Enemy : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player") != null)
        {
            collision.gameObject.SendMessage("TakeDamage", 10);

        }
    }
}
