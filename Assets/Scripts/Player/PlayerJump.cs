using System;
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

    public LayerMask whatIsGround;

    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        checkRunning = GetComponent<Player>();
        anim = GetComponent<Animator>();
    }
    
    void Update()
    {
        CheckCollision();
        checkInput();
        if (isGrounded)
        {
            canDoubleJump = true;
        }

        
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
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, Vector2.zero, 0, whatIsGround);
    }

    private void checkInput()
    {
    
        if (jumpAction.WasPressedThisFrame() && checkRunning.IsRunBegun && !wallDetected&& !checkRunning.IsSliding)
        {

            Jump();
            
        }
    }

    

    void Jump()
    {
        if (isGrounded)
        {
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            print("player is jumpping");
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            print("player is jumpping two time");
        }
        
    }
    // vẽ tia raycast
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
