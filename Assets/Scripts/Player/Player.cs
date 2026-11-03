using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("speed information")]
    [SerializeField] bool isRunBegun;
    [SerializeField] float moveSpeed;

    [Header("input information")]
    [SerializeField] InputAction runAction;

    [Tooltip("Kéo thả Action SwipeDelta vào đây")]
    public InputActionReference swipeAction;
    [Tooltip("Độ dài vuốt tối thiểu để kích hoạt trượt (lọc nhiễu)")]
    public float swipeThreshold = 20f;

    [Header("slide information")]
    [SerializeField] float slideSpeed;
    [SerializeField] float slideCounter;
    [SerializeField] bool isSliding = false;
    private Vector2 originalSize;
    private Vector2 originalOffset;
    private CapsuleCollider2D body;
    [SerializeField] LayerMask whatIsGround;

    [Header("check box")]
    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;

    private Animator anim;
    private Rigidbody2D rb;


    public bool IsRunBegun { get => isRunBegun; private set => isRunBegun = value; }
    public bool IsSliding { get => isSliding; set => isSliding = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        body = GetComponent<CapsuleCollider2D>();
        originalSize = body.size;
        originalOffset = body.offset;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        checkInput();


        if (IsRunBegun)
        {
            Running();
        }

        AnimatorController();
    }

    private void Running()
    {
        if (isSliding)
        {
            rb.linearVelocity = new Vector2(slideSpeed, rb.linearVelocity.y);

        }
        else
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);


        }
    }

    private void AnimatorController()
    {
        isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        anim.SetBool("isRunning", isRunning);


    }

    private void OnEnable()
    {
        runAction.Enable();
        swipeAction.action.Enable();
        swipeAction.action.performed += OnSwipe;
    }


    private void OnDisable()
    {
        runAction.Disable();
        swipeAction.action.performed -= OnSwipe;
        swipeAction.action.Disable();
    }
    private void checkInput()
    {
        if (runAction.WasPressedThisFrame())
        {
            IsRunBegun = true;
        }
    }

    private void OnSwipe(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();
        bool isJumping = rb.linearVelocity.y > 0.1f;
        if (delta.y < -swipeThreshold && !isSliding && !isJumping)
        {
            StartCoroutine(PerformSlide());
        }
    }
    private IEnumerator PerformSlide()
    {
        isSliding = true;
        body.offset = new Vector2(originalOffset.x, originalOffset.y - (originalSize.y / 4f));
        body.size = new Vector2(originalSize.x, originalSize.y / 2f);

        // Đợi tối thiểu slideCounter giây trước
        yield return new WaitForSeconds(slideCounter);
        // Sau đó kiểm tra từng frame: còn vật thể bên trên không?
        while (CeilingDetected())
        {
            yield return null; // đợi frame tiếp theo
        }
        isSliding = false;
        body.size = originalSize;
        body.offset = originalOffset;

    }
    // Kiểm tra vật thể phía trên đầu (vị trí đầu lúc bình thường)
    private bool CeilingDetected()
    {
        // Vị trí đỉnh đầu khi collider bình thường
        Vector2 ceilingCheckPos = (Vector2)transform.position
                                + Vector2.up * (originalOffset.y + originalSize.y / 2f);

        return Physics2D.OverlapCircle(ceilingCheckPos, originalSize.x / 2f, whatIsGround);
    }


}