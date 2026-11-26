using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerJump : MonoBehaviour
{
    #region Fields & Properties
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");

    [SerializeField] private float jumpForce = 20f;

    [Header("Collision Info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private InputAction jumpAction;

    [FormerlySerializedAs("checkRunning")]
    [SerializeField] private Player player;

    [SerializeField] private bool isGrounded;
    public LayerMask whatIsGround;

    private bool canDoubleJump;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private Animator anim;
    private PlayerSlide playerSlide;
    private PlayerLedgeClimb playerLedgeClimb;

    public bool IsGrounded => isGrounded;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        playerSlide = GetComponent<PlayerSlide>();
        playerLedgeClimb = GetComponent<PlayerLedgeClimb>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();
    }

    private void OnEnable()
    {
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        jumpAction?.Disable();
    }

    private void Update()
    {
        CheckCollision();
        CheckInput();
        UpdateAnimator();
    }
    #endregion

    #region Ground Check & Jump Logic
    private float GetGroundRayDistance()
    {
        if (groundCheckDistance > 0f)
        {
            return groundCheckDistance;
        }

        CapsuleCollider2D capsule = col != null ? col : GetComponent<CapsuleCollider2D>();
        return capsule != null ? Mathf.Max((capsule.size.y * 0.5f) + 0.05f, 0.5f) : 0.5f;
    }

    private void CheckCollision()
    {
        float distance = GetGroundRayDistance();
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, distance, whatIsGround);
    }

    private void CheckInput()
    {
        if (jumpAction == null || !jumpAction.WasPressedThisFrame()) return;
        if (player == null || !player.IsRunBegun || player.WallDetected) return;
        if (playerSlide != null && playerSlide.IsSliding) return;
        if (playerLedgeClimb != null && playerLedgeClimb.IsClimbing) return;

        Jump();
    }

    private void Jump()
    {
        if (isGrounded)
        {
            canDoubleJump = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
    #endregion

    #region Animation & Gizmos
    private void UpdateAnimator()
    {
        if (anim == null) return;

        anim.SetBool(IsGroundedHash, isGrounded);
        anim.SetFloat(YVelocityHash, rb.linearVelocity.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * GetGroundRayDistance());
    }
    #endregion
}
