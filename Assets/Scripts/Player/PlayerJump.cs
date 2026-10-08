using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] float jumpForce = 20f;
    [SerializeField] int maxJumpCount = 2; // 1 = nhảy thường, 2 = double jump
    private Rigidbody2D rb;
    private int jumpCount = 0;

    [Header("Collision infor")]
    [SerializeField] bool isGrounded;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] Player checkRunning;

    private Animator anim;
    private Collider2D col;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        checkRunning = GetComponent<Player>();
        anim = GetComponent<Animator>();
        isGrounded = true;
    }
    
    void Update()
    {
        CheckGrounded();

        if (jumpAction.WasPressedThisFrame() && jumpCount < maxJumpCount && checkRunning.IsRunBegun) 
        {
            //Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame && jumpCount < maxJumpCount
            Jump();
            print("player is jumpping");
        }

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private void CheckGrounded()
    {
        RaycastHit2D[] hits = new RaycastHit2D[5];
        int hitCount = col.Cast(Vector2.down, hits, groundCheckDistance);

        bool grounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].collider != null && hits[i].collider.CompareTag("Ground"))
            {
                grounded = true;
                break;
            }
        }

        // Khi đang nhảy lên (vận tốc Y dương đáng kể), không coi là đang chạm đất
        if (jumpCount > 0 && rb.linearVelocity.y > 0.1f)
        {
            grounded = false;
        }

        isGrounded = grounded;
        if (isGrounded)
        {
            jumpCount = 0;
        }
    }

    private void OnEnable()
    {
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
        jumpCount++;
    }
}
