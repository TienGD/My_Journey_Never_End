using System.Collections;
using UnityEngine;

/// <summary>
/// Script Leo Mép Tường (Ledge Climb) cho phép nhân vật tự động trèo lên tường
/// khi nhảy hơi thụt so với mép tường một khoảng cho phép.
/// Được điều phối gọi trực tiếp từ Player.Update() trước bước kiểm tra va chạm tường.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerLedgeClimb : MonoBehaviour
{
    #region Fields & Properties
    [Header("Ledge Detection Window (Khoảng nhảy thụt cho phép)")]
    [Tooltip("Độ cao Tia Trên (ngang đỉnh đầu) tính từ chân. Phía trên tia này phải thoáng mới trèo được.")]
    [SerializeField] private float upperRayHeight = 3.1f;

    [Tooltip("Độ cao Tia Dưới (ngang bụng/ngực) tính từ chân. Mép tường lọt vào giữa Tia Dưới và Tia Trên sẽ tự động trèo lên.")]
    [SerializeField] private float lowerRayHeight = 1.2f;

    [Tooltip("Khoảng cách vươn tay bắt mép tường tính từ mặt trước Collider.")]
    [SerializeField] private float detectDistance = 0.65f;

    [Tooltip("Layer của mặt đất / tường có thể leo lên.")]
    [SerializeField] private LayerMask whatIsGround;

    [Header("Climb Settings (Thông số trèo tường)")]
    [Tooltip("Tổng thời gian kéo nhân vật trèo lên đỉnh tường (giây).")]
    [SerializeField] private float climbDuration = 0.18f;

    [Tooltip("Khoảng cách bước chân vào bên trong mặt tường sau khi trèo xong.")]
    [SerializeField] private float standForwardOffset = 0.4f;

    [Tooltip("Thời gian nghỉ ngắn sau khi trèo xong để chống bắt lặp lại mép vừa trèo.")]
    [SerializeField] private float climbCooldown = 0.15f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private PlayerJump playerJump;
    private PlayerSlide playerSlide;

    private bool isClimbing;
    private float nextClimbAllowedTime;
    private float defaultGravityScale;

    public bool IsClimbing => isClimbing;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        playerJump = GetComponent<PlayerJump>();
        playerSlide = GetComponent<PlayerSlide>();

        defaultGravityScale = rb.gravityScale;

        if (whatIsGround.value == 0 && playerJump != null)
        {
            whatIsGround = playerJump.whatIsGround;
        }
    }

    private void OnDisable()
    {
        if (isClimbing)
        {
            isClimbing = false;
            if (col != null) col.enabled = true;
            if (rb != null) rb.gravityScale = defaultGravityScale;
        }
    }
    #endregion

    #region Ledge Detection & Climb
    public void CheckAndTriggerLedgeClimb()
    {
        // 1. Cắt sớm (Early-Exit) nếu đang trèo, đang hồi cooldown, hoặc đang trượt
        if (isClimbing || Time.time < nextClimbAllowedTime) return;
        if (playerSlide != null && playerSlide.IsSliding) return;

        // Nếu chân đang chạm mặt đất thì thoát ngay
        Vector2 feetOrigin = (Vector2)transform.position + new Vector2(col.offset.x, 0.05f);
        if (Physics2D.Raycast(feetOrigin, Vector2.down, 0.15f, whatIsGround)) return;

        // 2. Lazy Raycasting: Chỉ bắn tia tiếp theo khi tia trước thỏa mãn
        float centerX = transform.position.x + col.offset.x;
        float halfWidth = col.size.x * 0.5f;
        float horizontalRayLength = halfWidth + detectDistance;
        float playerY = transform.position.y;

        // TIA 1 (Tia Ngang Trên): Kiểm tra phía trên mép tường có thoáng không
        Vector2 upperOrigin = new Vector2(centerX, playerY + upperRayHeight);
        if (Physics2D.Raycast(upperOrigin, Vector2.right, horizontalRayLength, whatIsGround).collider != null)
        {
            return;
        }

        // TIA 2 (Tia Dọc Từ Trên Xuống): Quét trong cửa sổ [lowerRayHeight -> upperRayHeight]
        float windowHeight = upperRayHeight - lowerRayHeight;
        if (windowHeight <= 0f) return;

        float nearProbeX = centerX + halfWidth + 0.15f;
        float farProbeX = centerX + halfWidth + (detectDistance * 0.75f);

        RaycastHit2D topHit = Physics2D.Raycast(new Vector2(nearProbeX, playerY + upperRayHeight), Vector2.down, windowHeight, whatIsGround);
        if (topHit.collider == null || topHit.normal.y < 0.7f)
        {
            topHit = Physics2D.Raycast(new Vector2(farProbeX, playerY + upperRayHeight), Vector2.down, windowHeight, whatIsGround);
        }

        if (topHit.collider == null || topHit.normal.y < 0.7f) return;

        float ledgeTopY = topHit.point.y;

        // TIA 3 (Tia Ngang Dưới): Xác định tọa độ mép dọc X của tường
        Vector2 wallRayOrigin = new Vector2(centerX, ledgeTopY - 0.05f);
        RaycastHit2D wallHit = Physics2D.Raycast(wallRayOrigin, Vector2.right, horizontalRayLength, whatIsGround);
        float wallX = wallHit.collider != null ? wallHit.point.x : (centerX + halfWidth);

        Vector2 targetStandPos = new Vector2(wallX + standForwardOffset, ledgeTopY + 0.02f);

        // Kiểm tra khoảng trống trên đỉnh tường có đủ chỗ cho nhân vật đứng không
        Vector2 standBoxCenter = targetStandPos + new Vector2(col.offset.x, col.offset.y + 0.05f);
        Vector2 standBoxSize = new Vector2(col.size.x * 0.8f, col.size.y * 0.9f);
        if (Physics2D.OverlapBox(standBoxCenter, standBoxSize, 0f, whatIsGround))
        {
            return;
        }

        StartCoroutine(ClimbLedgeRoutine(targetStandPos));
    }

    private IEnumerator ClimbLedgeRoutine(Vector2 targetStandPos)
    {
        isClimbing = true;

        defaultGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        Vector2 startPos = transform.position;
        Vector2 pullUpPos = new Vector2(startPos.x, targetStandPos.y);

        float pullUpDuration = climbDuration * 0.6f;
        float stepForwardDuration = climbDuration * 0.4f;

        // Nhịp 1: Kéo người thẳng lên
        float elapsed = 0f;
        while (elapsed < pullUpDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, pullUpPos, Mathf.Clamp01(elapsed / pullUpDuration));
            yield return null;
        }

        // Nhịp 2: Bước chân lên mặt tường
        elapsed = 0f;
        while (elapsed < stepForwardDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(pullUpPos, targetStandPos, Mathf.Clamp01(elapsed / stepForwardDuration));
            yield return null;
        }

        transform.position = targetStandPos;
        col.enabled = true;
        rb.gravityScale = defaultGravityScale;

        isClimbing = false;
        nextClimbAllowedTime = Time.time + climbCooldown;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        CapsuleCollider2D capsule = col != null ? col : GetComponent<CapsuleCollider2D>();
        if (capsule == null) return;

        float centerX = transform.position.x + capsule.offset.x;
        float halfWidth = capsule.size.x * 0.5f;
        float horizontalRayLength = halfWidth + detectDistance;
        float playerY = transform.position.y;

        // 1. Tia Trên (Xanh lá - Phải thoáng)
        Vector3 upperStart = new Vector3(centerX, playerY + upperRayHeight, 0f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(upperStart, upperStart + (Vector3.right * horizontalRayLength));

        // 2. Tia Dưới (Xanh dương - Giới hạn dưới của khoảng nhảy thụt)
        Vector3 lowerStart = new Vector3(centerX, playerY + lowerRayHeight, 0f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(lowerStart, lowerStart + (Vector3.right * horizontalRayLength));

        // 3. Tia Dọc Quét Mép Tường (Màu vàng cam)
        float farProbeX = centerX + halfWidth + (detectDistance * 0.75f);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(
            new Vector3(farProbeX, playerY + upperRayHeight, 0f),
            new Vector3(farProbeX, playerY + lowerRayHeight, 0f)
        );
    }
    #endregion
}
