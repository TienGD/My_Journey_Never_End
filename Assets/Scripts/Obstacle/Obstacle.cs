using UnityEngine;

/// <summary>
/// Gắn vào từng obstacle prefab.
/// Obstacle hoạt động như một khối rắn (solid collider, tag "Ground") để nhân vật có thể đứng hoặc leo lên.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Chiều cao obstacle (units). Phải bằng chiều cao thực tế của sprite/collider.")]
    [SerializeField] private float obstacleHeight = 1.5f;

    public float ObstacleHeight => obstacleHeight;

    private void Awake()
    {
        gameObject.tag = "Ground";

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.isTrigger = false;
            col.usedByEffector = false;
        }
    }

    /// <summary>
    /// Đặt vị trí Y sao cho đáy của obstacle nằm tại bottomY.
    /// </summary>
    public void AlignToGround(float bottomY)
    {
        float targetY = bottomY + (obstacleHeight * 0.5f);
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }
}
