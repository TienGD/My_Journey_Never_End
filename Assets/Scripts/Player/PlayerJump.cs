using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] float jumpForce = 20f;
    private bool canDoubleJump;
    private Rigidbody2D rb;

    [Header("Collision infor")]

    [SerializeField] private float groundCheckDistance;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] Player checkRunning;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    [SerializeField] bool isGrounded;
    private bool wallDetected;
    private CapsuleCollider2D col;


    public LayerMask whatIsGround;

    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        checkRunning = GetComponent<Player>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        CheckCollision();
        checkInput();



        AnimationController();
    }

    private void AnimationController()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }


    private void OnEnable()
    {
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
    }

    private void CheckCollision()
    {
        float distance = col.size.y / 2f + 0.05f;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, distance, whatIsGround);
        wallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, Vector2.zero, 0, whatIsGround);
    }

    private void checkInput()
    {

        if (jumpAction.WasPressedThisFrame() && checkRunning.IsRunBegun && !wallDetected && !checkRunning.IsSliding)
        {

            Jump();

        }
    }



    void Jump()
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
    // vẽ tia raycast
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
