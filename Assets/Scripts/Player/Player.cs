using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region Fields & Properties
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");

    [Header("Movement & Speed")]
    [SerializeField] private bool isRunBegun;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    [Space]
    [SerializeField] private float milestoneIncreaser;

    [Header("Knockback Info")]
    [SerializeField] private Vector2 knockbackDirection;
    [SerializeField] private bool isKnock;
    [SerializeField] private bool canBeKnock = true;
    [SerializeField] private SpriteRenderer sr;

    [Header("Wall Collision Check")]
    [SerializeField] private bool isRunning;
    [SerializeField] private float wallCheckWidth = 0.08f;
    [SerializeField, Range(0.2f, 0.9f)] private float wallCheckHeightRatio = 0.6f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Input Information")]
    [SerializeField] private InputAction runAction;

    private float defaultSpeed;
    private float speedMilestone;
    private float defaultMilestoneIncreaser;
    private bool wallDetected;
    private bool isDead;

    private Animator anim;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private PlayerJump playerJump;
    private PlayerSlide playerSlide;
    private PlayerLedgeClimb playerLedgeClimb;

    public bool WallDetected { get => wallDetected; set => wallDetected = value; }
    public bool IsRunBegun { get => isRunBegun; private set => isRunBegun = value; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        playerJump = GetComponent<PlayerJump>();
        playerSlide = GetComponent<PlayerSlide>();
        playerLedgeClimb = GetComponent<PlayerLedgeClimb>();

        DontDestroyOnLoad(gameObject);

        if (whatIsGround.value == 0 && playerJump != null)
        {
            whatIsGround = playerJump.whatIsGround;
        }

        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultMilestoneIncreaser = milestoneIncreaser;
    }

    private void OnEnable()
    {
        runAction?.Enable();
    }

    private void OnDisable()
    {
        runAction?.Disable();
    }

    private void Update()
    {
        if (isKnock || isDead)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        if (playerLedgeClimb != null)
        {
            playerLedgeClimb.CheckAndTriggerLedgeClimb();
        }

        // Khi đang trèo tường thì nhường quyền di chuyển cho PlayerLedgeClimb
        bool isClimbing = playerLedgeClimb != null && playerLedgeClimb.IsClimbing;

        CheckWallCollision(isClimbing);

        if (wallDetected)
        {
            KnockBack();
            ResetSpeed();
            return;
        }

        CheckInput();

        if (IsRunBegun && !isClimbing)
        {
            UpdateRunningVelocity();
        }

        UpdateSpeedMilestone();
        UpdateAnimator();
    }
    #endregion

    #region Movement & Speed
    private void CheckInput()
    {
        if (runAction != null && runAction.WasPressedThisFrame())
        {
            IsRunBegun = true;
        }
    }

    private void UpdateRunningVelocity()
    {
        float currentSpeed = (playerSlide != null && playerSlide.IsSliding)
            ? playerSlide.SlideSpeed
            : moveSpeed;

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void UpdateSpeedMilestone()
    {
        if (moveSpeed >= maxSpeed)
        {
            return;
        }

        if (transform.position.x > speedMilestone)
        {
            speedMilestone += milestoneIncreaser;
            moveSpeed = Mathf.Min(moveSpeed * speedMultiplier, maxSpeed);
            milestoneIncreaser *= speedMultiplier;
        }
    }

    private void ResetSpeed()
    {
        moveSpeed = defaultSpeed;
        milestoneIncreaser = defaultMilestoneIncreaser;
        speedMilestone = transform.position.x + defaultMilestoneIncreaser;
    }
    #endregion

    #region Collision, Knockback & Death
    private void CheckWallCollision(bool isClimbing)
    {
        if (isClimbing || col == null || !col.enabled)
        {
            wallDetected = false;
            return;
        }

        GetWallCheckBox(col, out Vector2 boxCenter, out Vector2 boxSize);
        wallDetected = Physics2D.OverlapBox(boxCenter, boxSize, 0f, whatIsGround);
    }

    private void GetWallCheckBox(CapsuleCollider2D capsule, out Vector2 boxCenter, out Vector2 boxSize)
    {
        boxSize = new Vector2(wallCheckWidth, capsule.size.y * wallCheckHeightRatio);
        float frontEdgeX = transform.position.x + capsule.offset.x + (capsule.size.x * 0.5f);
        float centerY = transform.position.y + capsule.offset.y;
        boxCenter = new Vector2(frontEdgeX + (wallCheckWidth * 0.5f), centerY);
    }

    private void KnockBack()
    {
        if (!canBeKnock)
        {
            return;
        }

        isKnock = true;
        rb.linearVelocity = knockbackDirection;
        StartCoroutine(KnockbackAndInvincibilityRoutine());
    }

    private IEnumerator KnockbackAndInvincibilityRoutine()
    {
        Color originalColor = sr.color;
        Color darkenColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

        canBeKnock = false;

        sr.color = darkenColor;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
        yield return new WaitForSeconds(0.1f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(0.25f);
        sr.color = originalColor;
        yield return new WaitForSeconds(0.25f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(0.35f);
        sr.color = originalColor;
        yield return new WaitForSeconds(0.3f);

        sr.color = originalColor;
        isKnock = false;
        canBeKnock = true;
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        yield return new WaitForSeconds(1f);
        Debug.Log("Player is dead");
        rb.linearVelocity = Vector2.zero;
        IsRunBegun = false;
    }
    #endregion

    #region Animation & Gizmos
    private void UpdateAnimator()
    {
        isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        if (anim != null)
        {
            anim.SetBool(IsRunningHash, isRunning);
        }
    }

    private void OnDrawGizmos()
    {
        CapsuleCollider2D capsule = col != null ? col : GetComponent<CapsuleCollider2D>();
        if (capsule == null) return;

        GetWallCheckBox(capsule, out Vector2 boxCenter, out Vector2 boxSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
    #endregion
}
