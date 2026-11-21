using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Information")]
    [SerializeField] private float slideSpeed = 20f;
    [SerializeField] private float slideCounter = 0.3f;
    [SerializeField] private bool isSliding = false;
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
    private PlayerJump playerJump;

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

    private void Awake()
    {
        body = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerJump = GetComponent<PlayerJump>();

        // Tự động đồng bộ LayerMask mặt đất từ PlayerJump nếu chưa gán trong Inspector
        if (whatIsGround.value == 0 && playerJump != null)
        {
            whatIsGround = playerJump.whatIsGround;
        }

        CacheColliderGeometry();

        cachedSlideCounter = slideCounter;
        slideWait = new WaitForSeconds(slideCounter);

        // Khởi tạo phím trượt mặc định (Mũi tên xuống & S) nếu chưa gắn trong Inspector
        if (slideAction == null || slideAction.bindings.Count == 0)
        {
            slideAction = new InputAction("Slide", InputActionType.Button);
            slideAction.AddBinding("<Keyboard>/downArrow");
            slideAction.AddBinding("<Keyboard>/s");
        }
    }

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

    private void CheckSlideInput()
    {
        bool isSlideKeyPressed = (slideAction != null && slideAction.WasPressedThisFrame())
            || (Keyboard.current != null && (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame));

        if (isSlideKeyPressed)
        {
            TriggerSlide();
            return;
        }

        CheckSwipeInput();
    }

    // Gộp chung xử lý vuốt Chuột (PC) và Cảm ứng (Mobile) qua Pointer.current
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

    public void TriggerSlide()
    {
        bool inAir = playerJump != null
            ? !playerJump.IsGrounded
            : (rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.1f);

        if (!isSliding && !inAir)
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
            anim.SetBool("isSliding", true);
        }

        if (!Mathf.Approximately(cachedSlideCounter, slideCounter))
        {
            cachedSlideCounter = slideCounter;
            slideWait = new WaitForSeconds(slideCounter);
        }

        // Đợi tối thiểu slideCounter giây trước
        yield return slideWait;

        // Kiểm tra từng frame xem phía trên còn vật cản không trước khi đứng dậy
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
            anim.SetBool("isSliding", false);
        }
    }

    private bool CeilingDetected()
    {
        Vector2 checkCenter = (Vector2)transform.position + ceilingBoxOffset;
        return Physics2D.OverlapBox(checkCenter, ceilingBoxSize, 0f, whatIsGround);
    }

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
}
