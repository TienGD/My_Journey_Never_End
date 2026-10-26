using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý spawn chướng ngại vật ngẫu nhiên trên đường chạy của player.
/// Đảm bảo mọi obstacle đều có thể nhảy qua được.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerJump playerJump;
    [Tooltip("Camera chính của game — dùng để xác định cạnh phải màn hình")]
    [SerializeField] private Camera cam;

    [Header("Obstacle Prefabs")]
    [Tooltip("Danh sách prefab chướng ngại vật. Mỗi prefab phải có component Obstacle.")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách tối thiểu giữa 2 obstacle liên tiếp (units)")]
    [SerializeField] private float minSpawnGap = 20f;
    [Tooltip("Khoảng cách tối đa giữa 2 obstacle liên tiếp (units)")]
    [SerializeField] private float maxSpawnGap = 40f;
    [Tooltip("Khoảng cách buffer ngoài cạnh phải camera — obstacle luôn spawn sau vị trí này (units)")]
    [SerializeField] private float spawnBuffer = 3f;
    [Tooltip("Số obstacle tối đa đang active cùng lúc (2~3 là đủ)")]
    [SerializeField] [Range(2, 5)] private int maxActiveObstacles = 3;
    [Tooltip("Khoảng cách phía sau camera trái để xoá obstacle")]
    [SerializeField] private float despawnBehindCamera = 5f;

    [Header("Obstacle Height Settings")]
    [Tooltip("Chiều cao nhân vật (units) - obstacle tối thiểu phải cao bằng đầu nhân vật để buộc phải nhảy")]
    [SerializeField] private float playerHeight = 3.6f;
    [Tooltip("Lực nhảy (jumpForce) của player - phải khớp với PlayerJump.jumpForce")]
    [SerializeField] private float playerJumpForce = 15f;
    [Tooltip("Gravity scale của Rigidbody2D trên player")]
    [SerializeField] private float gravityScale = 1f;
    [Tooltip("Hệ số an toàn cho chiều cao tối đa (0~1). Giảm nếu obstacle quá khó nhảy qua)")]
    [SerializeField] [Range(0.5f, 0.95f)] private float safetyFactor = 0.8f;

    // Tính toán ở runtime
    private float minObstacleHeight; // = playerHeight
    private float maxObstacleHeight; // = chiều cao nhảy 2 lần × safetyFactor

    // Vị trí X tiếp theo sẽ spawn obstacle
    private float nextSpawnX;

    // Danh sách obstacles đang hoạt động trên scene
    private List<GameObject> activeObstacles = new List<GameObject>();

    private void Start()
    {
        // Tự lấy Camera.main nếu chưa gán trong Inspector
        if (cam == null) cam = Camera.main;

        // --- Tính chiều cao nhảy đơn: h = v² / (2g) ---
        float gravity = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
        float singleJumpHeight = (playerJumpForce * playerJumpForce) / (2f * gravity);

        // --- Tính chiều cao nhảy 2 lần (double jump tối đa) ---
        float doubleJumpHeight = 2f * singleJumpHeight;

        // --- Min/Max obstacle height ---
        minObstacleHeight = playerHeight;
        maxObstacleHeight = doubleJumpHeight * safetyFactor;

        Debug.Log($"[ObstacleSpawner] Single jump: {singleJumpHeight:F1}u | Double jump: {doubleJumpHeight:F1}u | " +
                  $"Obstacle range: [{minObstacleHeight:F1} ~ {maxObstacleHeight:F1}] units");

        // Bắt đầu spawn từ ngay ngoài cạnh phải camera
        nextSpawnX = GetCameraRightEdge() + spawnBuffer;
    }

    private void Update()
    {
        SpawnObstaclesAhead();
        DespawnObstaclesBehind();
    }

    /// <summary>
    /// Spawn obstacle khi:
    ///   1. Số obstacle active còn dưới maxActiveObstacles
    ///   2. Vị trí spawn (nextSpawnX) nằm ngoài cạnh phải camera
    /// → Player không bao giờ thấy obstacle xuất hiện trong màn hình.
    /// </summary>
    private void SpawnObstaclesAhead()
    {
        // Dọn null trước khi đếm
        activeObstacles.RemoveAll(o => o == null);

        // Chỉ spawn thêm khi chưa đủ số lượng tối đa
        if (activeObstacles.Count >= maxActiveObstacles) return;

        float cameraRight = GetCameraRightEdge();

        // nextSpawnX phải nằm ngoài màn hình (sau cạnh phải camera + buffer)
        float safeSpawnX = cameraRight + spawnBuffer;
        if (nextSpawnX < safeSpawnX)
            nextSpawnX = safeSpawnX;

        // Spawn cho đến khi đủ maxActiveObstacles
        while (activeObstacles.Count < maxActiveObstacles)
        {
            TrySpawnObstacle(nextSpawnX);
            nextSpawnX += Random.Range(minSpawnGap, maxSpawnGap);
        }
    }

    /// <summary>
    /// Xoá obstacle khi đã ra khỏi cạnh TRÁI camera (player đã vượt qua).
    /// </summary>
    private void DespawnObstaclesBehind()
    {
        float cameraLeft = GetCameraLeftEdge();

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            // Xoá khi obstacle đã khuất hẳn bên trái camera
            if (activeObstacles[i].transform.position.x < cameraLeft - despawnBehindCamera)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Spawn một obstacle ngẫu nhiên tại vị trí X.
    /// Đáy obstacle được đặt tại groundY + playerHeight → obstacle nổi cao hơn đầu nhân vật,
    /// player phải nhảy lên mới chạm, khi rơi xuống bị obstacle cản (PlatformEffector2D one-way).
    /// Kích thước obstacle giữ nguyên như trong prefab.
    /// </summary>
    private void TrySpawnObstacle(float posX)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("[ObstacleSpawner] Không có obstacle prefab nào được gán!");
            return;
        }

        // Random chọn prefab
        GameObject chosenPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (chosenPrefab == null) return;

        // Tìm Y mặt đất tại vị trí spawn
        float groundY = GetGroundY(posX);

        // Đáy obstacle nằm tại groundY + playerHeight (cao hơn đầu nhân vật)
        float bottomY = groundY + playerHeight;

        // Spawn tạm (AlignToGround sẽ đặt đúng vị trí Y ngay sau)
        GameObject obstacleObj = Instantiate(chosenPrefab, new Vector3(posX, bottomY, 0f), Quaternion.identity);

        Obstacle obs = obstacleObj.GetComponent<Obstacle>();
        if (obs != null)
        {
            // Đặt đáy obstacle tại groundY + playerHeight
            obs.AlignToGround(bottomY);
        }

        activeObstacles.Add(obstacleObj);
    }

    /// <summary>Trả về toạ độ X cạnh PHẢI camera trong world space.</summary>
    private float GetCameraRightEdge()
    {
        if (cam == null) return player.position.x + 20f;
        // ViewportToWorldPoint(1,0.5,0) = điểm giữa cạnh phải viewport
        return cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
    }

    /// <summary>Trả về toạ độ X cạnh TRÁI camera trong world space.</summary>
    private float GetCameraLeftEdge()
    {
        if (cam == null) return player.position.x - 20f;
        return cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
    }

    /// <summary>
    /// Trả về toạ độ Y của bề mặt đất tại vị trí X cho trước.
    /// Dùng 3 tầng fallback để đảm bảo luôn tìm được mặt đất:
    ///   1. Raycast với LayerMask "Ground"
    ///   2. Raycast không có LayerMask (tìm bất kỳ collider nào)
    ///   3. Fallback cứng dựa vào vị trí player
    /// </summary>
    private float GetGroundY(float posX)
    {
        Vector2 rayOrigin = new Vector2(posX, player.position.y + 5f); // Bắt đầu từ trên player
        float rayLength = 50f;

        // --- Tầng 1: Raycast với layer "Ground" ---
        int groundLayerMask = LayerMask.GetMask("Ground");
        if (groundLayerMask != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayerMask);
            if (hit.collider != null)
            {
                return hit.point.y;
            }
        }

        // --- Tầng 2: Raycast không có LayerMask (bỏ qua layer của player) ---
        int playerLayer = player.gameObject.layer;
        int allExceptPlayer = ~(1 << playerLayer);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, rayLength, allExceptPlayer);
        foreach (RaycastHit2D h in hits)
        {
            // Bỏ qua chính obstacle đang kiểm tra và các trigger
            if (h.collider != null && !h.collider.isTrigger && h.collider.gameObject != gameObject)
            {
                return h.point.y;
            }
        }

        // --- Tầng 3: Fallback dựa vào vị trí player ---
        // Ước tính Y mặt đất = vị trí player Y - nửa chiều cao CapsuleCollider (≈ 1.8 units)
        Debug.LogWarning($"[ObstacleSpawner] Không tìm được mặt đất tại X={posX:F1}. Dùng fallback từ vị trí player.");
        return player.position.y - 1.8f;
    }

#if UNITY_EDITOR
    /// <summary>Vẽ Gizmos để debug khoảng spawn trong Scene view.</summary>
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // Cạnh phải camera (vùng spawn)
        Gizmos.color = Color.green;
        float rightEdge = Application.isPlaying ? GetCameraRightEdge() : player.position.x + 15f;
        Gizmos.DrawLine(
            new Vector3(rightEdge + spawnBuffer, player.position.y - 5f, 0),
            new Vector3(rightEdge + spawnBuffer, player.position.y + 5f, 0)
        );

        // Cạnh trái camera (vùng despawn)
        Gizmos.color = Color.red;
        float leftEdge = Application.isPlaying ? GetCameraLeftEdge() : player.position.x - 15f;
        Gizmos.DrawLine(
            new Vector3(leftEdge - despawnBehindCamera, player.position.y - 5f, 0),
            new Vector3(leftEdge - despawnBehindCamera, player.position.y + 5f, 0)
        );

        // Chiều cao obstacle tối đa (= double jump height × safetyFactor)
        if (Application.isPlaying)
        {
            // Min obstacle height (màu vàng)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(player.position.x - 5f, player.position.y - 1.8f + minObstacleHeight, 0),
                new Vector3(player.position.x + 5f, player.position.y - 1.8f + minObstacleHeight, 0)
            );
            // Max obstacle height (màu đỏ cam)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawLine(
                new Vector3(player.position.x - 5f, player.position.y - 1.8f + maxObstacleHeight, 0),
                new Vector3(player.position.x + 5f, player.position.y - 1.8f + maxObstacleHeight, 0)
            );
        }
    }
#endif
}
