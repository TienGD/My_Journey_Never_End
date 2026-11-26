using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý spawn và despawn chướng ngại vật ngẫu nhiên trên đường chạy của player.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    #region Fields
    [Header("References")]
    [SerializeField] private Transform player;
    [Tooltip("Camera chính của game — dùng để xác định cạnh màn hình")]
    [SerializeField] private Camera cam;

    [Header("Obstacle Prefabs")]
    [Tooltip("Danh sách prefab chướng ngại vật. Mỗi prefab nên có component Obstacle.")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách tối thiểu giữa 2 obstacle liên tiếp (units)")]
    [SerializeField] private float minSpawnGap = 20f;
    [Tooltip("Khoảng cách tối đa giữa 2 obstacle liên tiếp (units)")]
    [SerializeField] private float maxSpawnGap = 40f;
    [Tooltip("Khoảng cách buffer ngoài cạnh phải camera — obstacle luôn spawn sau vị trí này (units)")]
    [SerializeField] private float spawnBuffer = 3f;
    [Tooltip("Số obstacle tối đa đang active cùng lúc")]
    [SerializeField, Range(2, 5)] private int maxActiveObstacles = 3;
    [Tooltip("Khoảng cách phía sau camera trái để xoá obstacle")]
    [SerializeField] private float despawnBehindCamera = 5f;

    [Header("Obstacle Height Settings")]
    [Tooltip("Độ cao đáy obstacle tính từ mặt đất (units)")]
    [SerializeField] private float playerHeight = 3.6f;

    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private float nextSpawnX;
    private int groundLayerMask;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
    }

    private void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        nextSpawnX = GetCameraRightEdge() + spawnBuffer;
    }

    private void Update()
    {
        if (player == null) return;

        DespawnObstaclesBehind();
        SpawnObstaclesAhead();
    }
    #endregion

    #region Spawn & Despawn Logic
    private void SpawnObstaclesAhead()
    {
        if (activeObstacles.Count >= maxActiveObstacles) return;

        float safeSpawnX = GetCameraRightEdge() + spawnBuffer;
        if (nextSpawnX < safeSpawnX)
        {
            nextSpawnX = safeSpawnX;
        }

        while (activeObstacles.Count < maxActiveObstacles)
        {
            if (!TrySpawnObstacle(nextSpawnX))
            {
                break;
            }
            nextSpawnX += Random.Range(minSpawnGap, maxSpawnGap);
        }
    }

    private void DespawnObstaclesBehind()
    {
        float despawnX = GetCameraLeftEdge() - despawnBehindCamera;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obs = activeObstacles[i];
            if (obs == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            if (obs.transform.position.x < despawnX)
            {
                Destroy(obs);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    private bool TrySpawnObstacle(float posX)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("[ObstacleSpawner] Không có obstacle prefab nào được gán!");
            return false;
        }

        GameObject chosenPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (chosenPrefab == null) return false;

        float groundY = GetGroundY(posX);
        float bottomY = groundY + playerHeight;

        GameObject obstacleObj = Instantiate(chosenPrefab, new Vector3(posX, bottomY, 0f), Quaternion.identity);
        if (obstacleObj.TryGetComponent(out Obstacle obs))
        {
            obs.AlignToGround(bottomY);
        }

        activeObstacles.Add(obstacleObj);
        return true;
    }
    #endregion

    #region Camera & Ground Helpers
    private float GetCameraRightEdge()
    {
        if (cam == null) return player != null ? player.position.x + 20f : 20f;
        return cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
    }

    private float GetCameraLeftEdge()
    {
        if (cam == null) return player != null ? player.position.x - 20f : -20f;
        return cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
    }

    private float GetGroundY(float posX)
    {
        Vector2 rayOrigin = new Vector2(posX, player.position.y + 5f);
        const float RayLength = 50f;

        // Tầng 1: Raycast với layer "Ground"
        if (groundLayerMask != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, RayLength, groundLayerMask);
            if (hit.collider != null)
            {
                return hit.point.y;
            }
        }

        // Tầng 2: Raycast bỏ qua layer của player
        int allExceptPlayer = ~(1 << player.gameObject.layer);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, RayLength, allExceptPlayer);
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider != null && !h.collider.isTrigger && h.collider.gameObject != gameObject)
            {
                return h.point.y;
            }
        }

        // Tầng 3: Fallback dựa vào vị trí player
        Debug.LogWarning($"[ObstacleSpawner] Không tìm được mặt đất tại X={posX:F1}. Dùng fallback từ vị trí player.");
        return player.position.y - 1.8f;
    }
    #endregion

    #region Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        float rightEdge = Application.isPlaying ? GetCameraRightEdge() : player.position.x + 15f;
        Gizmos.DrawLine(
            new Vector3(rightEdge + spawnBuffer, player.position.y - 5f, 0f),
            new Vector3(rightEdge + spawnBuffer, player.position.y + 5f, 0f)
        );

        Gizmos.color = Color.red;
        float leftEdge = Application.isPlaying ? GetCameraLeftEdge() : player.position.x - 15f;
        Gizmos.DrawLine(
            new Vector3(leftEdge - despawnBehindCamera, player.position.y - 5f, 0f),
            new Vector3(leftEdge - despawnBehindCamera, player.position.y + 5f, 0f)
        );
    }
#endif
    #endregion
}
