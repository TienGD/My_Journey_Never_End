using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] float jumpForce = 20f;
    private bool canDoubleJump;
    private Rigidbody2D rb;

    [Header("Collision infor")]
    [SerializeField] bool isGrounded;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] Player checkRunning;
    public LayerMask whatIsGround;

    private Animator anim;
    private Collider2D col;

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

        //anim.SetBool("isGrounded", isGrounded);
        //anim.SetFloat("yVelocity", rb.linearVelocity.y);
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
    }

    private void checkInput()
    {
        if (jumpAction.WasPressedThisFrame() && checkRunning.IsRunBegun)
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
        
    }
}
