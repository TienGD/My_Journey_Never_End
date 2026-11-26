using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerSlide : MonoBehaviour
{
    #region Fields & Properties
    private static readonly int IsSlidingHash = Animator.StringToHash("isSliding");

    [Header("Slide Information")]
    [SerializeField] private float slideSpeed = 20f;
    [SerializeField] private float slideCounter = 0.3f;
    [SerializeField] private bool isSliding;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Input Information")]
    [SerializeField] private InputAction slideAction;
    [Tooltip("Kéo thả Action SwipeDelta vào đây")]
    public InputActionReference swipeAction;
    [Tooltip("Độ dài vuốt tối thiểu để kích hoạt trượt (lọc nhiễu)")]
    public float swipeThreshold = 24f;

    private CapsuleCollider2D body;
    private Rigidbody2D rb;
    private Animator anim;
    private Player player;
    private PlayerJump playerJump;
    private PlayerLedgeClimb playerLedgeClimb;

    // Các thông số hình học được tính sẵn 1 lần (Cache)
    private Vector2 originalSize;
    private Vector2 originalOffset;
    private Vector2 slideSize;
    private Vector2 slideOffset;
    private Vector2 ceilingBoxOffset;
    private Vector2 ceilingBoxSize;

    // Cache bộ đếm thời gian Coroutine để tránh cấp phát bộ nhớ (Zero GC Alloc)
    private WaitForSeconds slideWait;
    private float cachedSlideCounter;
    private Coroutine slideCoroutine;

    private Vector2 pointerDownPos;
    private bool isPointerHeld;

    public bool IsSliding => isSliding;
    public float SlideSpeed => slideSpeed;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        body = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
        playerJump = GetComponent<PlayerJump>();
        playerLedgeClimb = GetComponent<PlayerLedgeClimb>();

        if (whatIsGround.value == 0 && playerJump != null)
        {
            whatIsGround = playerJump.whatIsGround;
        }

        CacheColliderGeometry();

        cachedSlideCounter = slideCounter;
        slideWait = new WaitForSeconds(slideCounter);

        if (slideAction == null || slideAction.bindings.Count == 0)
        {
            slideAction = new InputAction("Slide", InputActionType.Button);
            slideAction.AddBinding("<Keyboard>/downArrow");
            slideAction.AddBinding("<Keyboard>/s");
        }
    }

    private void OnEnable()
    {
        slideAction?.Enable();
        if (swipeAction != null && swipeAction.action != null)
        {
            swipeAction.action.Enable();
            swipeAction.action.performed += OnSwipe;
        }
    }

    private void OnDisable()
    {
        slideAction?.Disable();
        if (swipeAction != null && swipeAction.action != null)
        {
            swipeAction.action.performed -= OnSwipe;
            swipeAction.action.Disable();
        }
    }

    private void Update()
    {
        CheckSlideInput();
    }
    #endregion

    #region Input Handling
    private void CheckSlideInput()
    {
        if (slideAction != null && slideAction.WasPressedThisFrame())
        {
            TriggerSlide();
            return;
        }

        CheckSwipeInput();
    }

    private void CheckSwipeInput()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
        {
            pointerDownPos = pointer.position.ReadValue();
            isPointerHeld = true;
        }
        else if (pointer.press.wasReleasedThisFrame)
        {
            isPointerHeld = false;
        }
        else if (isPointerHeld)
        {
            Vector2 currentPos = pointer.position.ReadValue();
            if (currentPos.y - pointerDownPos.y < -swipeThreshold)
            {
                TriggerSlide();
                isPointerHeld = false;
            }
        }
    }

    private void OnSwipe(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();
        if (delta.y < -swipeThreshold)
        {
            TriggerSlide();
        }
    }
    #endregion

    #region Slide Mechanics & Geometry
    private void CacheColliderGeometry()
    {
        originalSize = body.size;
        originalOffset = body.offset;

        // Thu nhỏ chiều cao đi 1/2, giữ nguyên đáy Collider ở chân
        slideSize = new Vector2(originalSize.x, originalSize.y * 0.5f);
        slideOffset = new Vector2(originalOffset.x, originalOffset.y - (originalSize.y * 0.25f));

        // Hộp kiểm tra trần nhà bao phủ toàn bộ nửa thân trên khi đứng dậy
        ceilingBoxOffset = new Vector2(originalOffset.x, originalOffset.y + (originalSize.y * 0.25f));
        ceilingBoxSize = new Vector2(originalSize.x * 0.9f, originalSize.y * 0.5f);
    }

    public void TriggerSlide()
    {
        if (isSliding) return;
        if (player != null && (!player.IsRunBegun || player.WallDetected)) return;
        if (playerLedgeClimb != null && playerLedgeClimb.IsClimbing) return;

        bool inAir = playerJump != null
            ? !playerJump.IsGrounded
            : (rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.1f);

        if (!inAir)
        {
            slideCoroutine = StartCoroutine(PerformSlide());
        }
    }

    public bool TryCancelSlide()
    {
        if (!isSliding) return true;
        if (CeilingDetected()) return false;

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }

        EndSlide();
        return true;
    }

    private IEnumerator PerformSlide()
    {
        isSliding = true;
        body.offset = slideOffset;
        body.size = slideSize;

        if (anim != null)
        {
            anim.SetBool(IsSlidingHash, true);
        }

        if (!Mathf.Approximately(cachedSlideCounter, slideCounter))
        {
            cachedSlideCounter = slideCounter;
            slideWait = new WaitForSeconds(slideCounter);
        }

        yield return slideWait;

        while (CeilingDetected())
        {
            yield return null;
        }

        EndSlide();
        slideCoroutine = null;
    }

    private void EndSlide()
    {
        isSliding = false;
        body.size = originalSize;
        body.offset = originalOffset;

        if (anim != null)
        {
            anim.SetBool(IsSlidingHash, false);
        }
    }

    private bool CeilingDetected()
    {
        Vector2 checkCenter = (Vector2)transform.position + ceilingBoxOffset;
        return Physics2D.OverlapBox(checkCenter, ceilingBoxSize, 0f, whatIsGround);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        CapsuleCollider2D col = body != null ? body : GetComponent<CapsuleCollider2D>();
        if (col == null) return;

        Vector2 size = body != null ? originalSize : col.size;
        Vector2 offset = body != null ? originalOffset : col.offset;

        Vector2 boxCenter = (Vector2)transform.position + new Vector2(offset.x, offset.y + (size.y * 0.25f));
        Vector2 boxSize = new Vector2(size.x * 0.9f, size.y * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
    #endregion
}
