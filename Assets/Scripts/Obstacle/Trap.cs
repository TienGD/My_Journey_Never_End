using UnityEngine;

public class Trap : MonoBehaviour
{
    protected private int damage = 1;
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            player.KnockBack(damage);
        }
    }
}
