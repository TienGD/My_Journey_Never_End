using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    [Header("speed information")]
    [SerializeField] bool isRunBegun;
    [SerializeField] float moveSpeed;

    [Header("input information")]
    [SerializeField] InputAction runAction;

    [Header("check box")]
    [SerializeField] bool isRunning;
    [SerializeField] bool isGrounded;

    [Header("Speed info")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private float defaultSpeed;
    private PlayerJump wallDetect;
    [Space]
    [SerializeField] private float milestoneIncreaser;
    private float speedMilestone;
    private float defaultMilestoneIncreaser;

    private Animator anim;
    private Rigidbody2D rb;
    private PlayerSlide playerSlide;
    private PlayerLedgeClimb playerLedgeClimb;

    public bool IsRunBegun { get => isRunBegun; private set => isRunBegun = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerSlide = GetComponent<PlayerSlide>();
        playerLedgeClimb = GetComponent<PlayerLedgeClimb>();
        DontDestroyOnLoad(gameObject);
        wallDetect = GetComponent<PlayerJump>();
        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultMilestoneIncreaser = milestoneIncreaser;
    }

    private void Update()
    {
        checkInput();

        // Khi đang trèo tường thì nhường quyền di chuyển cho PlayerLedgeClimb
        bool isClimbing = playerLedgeClimb != null && playerLedgeClimb.IsClimbing;
        if (IsRunBegun && !isClimbing)
        {
            Running();
        }

        

        SpeedController();

        AnimatorController();
    }

    private void SpeedController()
    {
        if (moveSpeed >= maxSpeed)
            return;

        if (transform.position.x > speedMilestone)
        {
            speedMilestone = speedMilestone + milestoneIncreaser;

            moveSpeed = moveSpeed * speedMultiplier;
            milestoneIncreaser = milestoneIncreaser * speedMultiplier;

            if (moveSpeed > maxSpeed)
                moveSpeed = maxSpeed;
        }
    }

    private void speedReset()
    {
        moveSpeed = defaultSpeed;
        milestoneIncreaser = defaultMilestoneIncreaser;
        speedMilestone = transform.position.x + defaultMilestoneIncreaser;

    }

    private void Running()
    {
        if (wallDetect != null)
        {
            if (wallDetect.WallDetected)
            {
                speedReset();
            }
        }
        float currentSpeed = (playerSlide != null && playerSlide.IsSliding)
            ? playerSlide.SlideSpeed
            : moveSpeed;

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void AnimatorController()
    {
        isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        anim.SetBool("isRunning", isRunning);
        if (playerSlide != null)
        {
            anim.SetBool("isSliding", playerSlide.IsSliding);
        }
    }

    private void OnEnable()
    {
        runAction?.Enable();
    }

    private void OnDisable()
    {
        runAction?.Disable();
    }

    private void checkInput()
    {
        if (runAction != null && runAction.WasPressedThisFrame())
        {
            IsRunBegun = true;
        }
    }
}
