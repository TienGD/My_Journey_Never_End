using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [Header("speed information")]
    [SerializeField] bool isRunBegun ;
    [SerializeField] float moveSpeed ;

    [Header("input information")]
    [SerializeField] InputAction runAction;

    [Tooltip("Kéo thả Action SwipeDelta vào đây")]
    public InputActionReference swipeAction;
    [Tooltip("Độ dài vuốt tối thiểu để kích hoạt trượt (lọc nhiễu)")]
    public float swipeThreshold = 20f;

    [Header("slide information")]
    [SerializeField] float slideSpeed;
    [SerializeField] float slideCounter;
    [SerializeField] bool isSliding=false;

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
            Debug.Log("is sliding");
        }
        else
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            Debug.Log("is running");

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

        if (delta.y < -swipeThreshold && !isSliding)
        {
            StartCoroutine(PerformSlide());
        }
    }
    private IEnumerator PerformSlide()
    {
        isSliding = true;
        Debug.Log("Bắt đầu trượt! (Thu nhỏ Collider, chạy Animation)");

        yield return new WaitForSeconds(slideCounter); 


        isSliding = false;
        Debug.Log("Kết thúc trượt!");
    }


}