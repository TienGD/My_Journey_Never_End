using UnityEngine;

public class TriggerDeadZone : MonoBehaviour
{
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.instance.RestartLevel();
    }
}
