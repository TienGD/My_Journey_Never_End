using UnityEngine;

/// <summary>
/// Gắn vào từng obstacle prefab.
/// - Collider chính (BoxCollider2D): IS TRIGGER = FALSE, Used By Effector = FALSE
/// - Obstacle hoạt động như một khối rắn bình thường, nhân vật có thể đứng trên đó (nhờ tag Ground).
/// Hàm AlignToGround() được ObstacleSpawner gọi sau khi spawn để đặt đúng vị trí Y.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Chiều cao obstacle (units). Phải bằng chiều cao thực tế của sprite/collider.")]
    [SerializeField] private float obstacleHeight = 1.5f;

    /// <summary>Chiều cao obstacle để ObstacleSpawner kiểm tra tính hợp lệ.</summary>
    public float ObstacleHeight => obstacleHeight;

    private void Awake()
    {
        // Gán tag Ground để PlayerJump nhận diện đây là mặt đất có thể nhảy tiếp
        gameObject.tag = "Ground";

        // Ép tất cả các collider trên object này thành khối rắn (solid)
        // Đề phòng trường hợp người dùng lỡ bật Is Trigger trong Inspector khiến nhân vật rớt xuyên qua
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.isTrigger = false;   // Phải là solid để block vật lý
            col.usedByEffector = false; // Không dùng one-way nữa
        }
    }

    /// <summary>
    /// Đặt vị trí Y sao cho ĐÁY của obstacle nằm tại bottomY.
    /// Dùng obstacleHeight/2 làm offset (tin cậy ngay sau Instantiate, không phụ thuộc Physics update).
    /// </summary>
    /// <param name="bottomY">Toạ độ Y nơi đáy obstacle sẽ được đặt.</param>
    public void AlignToGround(float bottomY)
    {
        // Pivot của obstacle ở giữa → offset từ pivot đến đáy = obstacleHeight / 2
        float halfHeight = obstacleHeight / 2f;
        float targetY = bottomY + halfHeight;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }
}
