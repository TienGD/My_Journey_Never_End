using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("speed information")]
    [SerializeField] bool isRunBegun ;
    [SerializeField] float moveSpeed ;

    private Rigidbody2D rb;

    [Header("input information")]
    [SerializeField] private InputAction runAction;

    private Animator anim;

    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;

    public bool IsRunBegun { get => isRunBegun; private set => isRunBegun = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        checkInput();

        if (IsRunBegun)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        }

        AnimatorController();
    }

    private void AnimatorController()
    {
        isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        anim.SetBool("isRunning", isRunning);
        
    }

    private void OnEnable()
    {
        runAction.Enable();
    }


    private void OnDisable()
    {
        runAction.Disable();
    }
    private void checkInput()
    {
        if (runAction.WasPressedThisFrame())
        {
            IsRunBegun = true;

        }
    }

}