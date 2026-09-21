using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float patrolWidth = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float idleTime = 1f;
    private Animator animator;
    private Rigidbody2D rb;

    private float leftPosition;
    private float rightPosition;

    private int moveDirection = 1;
    private bool isIdle;
    private float idleTimer;
    private Vector3 startingScale;

    [Header("Knockback")]
    [SerializeField] private float knockbackSpeed = 2f;
    [SerializeField] private float knockbackDuration = 0.1f;
    private bool isKnockback;
    private float knockbackEndTime;
    private float knockbackDirection;


    private bool isMovementPaused;
    private float movementPauseEndTime;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        startingScale = transform.localScale;

        float halfWidth = patrolWidth / 2f;
        leftPosition = transform.position.x - halfWidth;
        rightPosition = transform.position.x + halfWidth;

        StartRunning();
    }


    private void FixedUpdate()
    {
        if (isKnockback)
        {
            if (Time.time < knockbackEndTime)
            {
                rb.linearVelocity = new Vector2(
                    knockbackDirection * knockbackSpeed,
                    rb.linearVelocity.y
                );

                return;
            }

            isKnockback = false;
        }

        if (isMovementPaused)
        {
            if (Time.time < movementPauseEndTime)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            isMovementPaused = false;
        }

        if (isIdle)
        {
            WaitForTurning();
            return;
        }
        Run();
    }

    private void Run()
    {
        rb.linearVelocity = new Vector2(moveSpeed * moveDirection, rb.linearVelocity.y);
        animator.SetBool("isRunning", true);

        bool reachedLeft = moveDirection < 0 && transform.position.x <= leftPosition;
        bool reachedRight = moveDirection > 0 && transform.position.x >= rightPosition;

        if (reachedLeft || reachedRight)
        {
            StartIdle();
        }
    }

    private void WaitForTurning()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        idleTimer -= Time.fixedDeltaTime;
        if (idleTimer > 0f)
        {
            return;

        }

        moveDirection *= -1;
        StartRunning();
    }

    private void StartIdle()
    {
        isIdle = true;
        idleTimer = idleTime;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
    }

    private void StartRunning()
    {
        isIdle = false;

        rb.linearVelocity = new Vector2(
            moveSpeed * moveDirection,
            rb.linearVelocity.y
        );

        FlipSprite();
        animator.SetBool("isRunning", true);
    }

    private void FlipSprite()
    {
        if (Mathf.Abs(rb.linearVelocity.x) <= Mathf.Epsilon)
        {
            return;
        }

        float direction = -Mathf.Sign(rb.linearVelocity.x);

        transform.localScale = new Vector3(
            Mathf.Abs(startingScale.x) * direction, startingScale.y, startingScale.z
        );
    }


    public void ApplyKnockback(float direction)
    {
        // Store the direction for the entire knockback duration.
        knockbackDirection = Mathf.Sign(direction);

        isKnockback = true;

        knockbackEndTime = Time.time + knockbackDuration;

        animator.SetBool("isRunning", false);

        rb.linearVelocity = new Vector2(
            knockbackDirection * knockbackSpeed,
            rb.linearVelocity.y
        );
    }

    public void PauseMovement(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        isMovementPaused = true;
        movementPauseEndTime = Time.time + duration;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
    }
}
