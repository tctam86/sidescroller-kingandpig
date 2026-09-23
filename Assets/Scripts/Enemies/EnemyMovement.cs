using Unity.VisualScripting;
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

    [Header("Chase")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float loseRange = 4f;
    [SerializeField] private float attackStopDistance = 0.5f;
    [SerializeField] private float verticalDetectionRange = 1.5f;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private float groundCheckDistance = 0.25f;


    private CapsuleCollider2D bodyCollider;
    private bool isChasing;
    private bool isReturning;
    private float returnPositionX;

    private bool isMovementPaused;
    private float movementPauseEndTime;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        startingScale = transform.localScale;

        bodyCollider = GetComponent<CapsuleCollider2D>();

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

        UpdateChaseState();
        if (isChasing)
        {
            Chase();
            return;
        }

        if (isReturning)
        {
            ReturnToPatrol();
            return;
        }


        if (isIdle)
        {
            WaitForTurning();
            return;
        }
        Run();
    }

    private void UpdateChaseState()
    {
        if (isReturning)
        {
            return;
        }

        if (player == null)
        {
            StopChasingAndReturn();
            return;
        }

        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);

        if (!isChasing)
        {
            bool playerIsClose = horizontalDistance <= detectionRange && verticalDistance <= verticalDetectionRange;
            if (playerIsClose)
            {
                isChasing = true;
                isReturning = false;
                isIdle = false;
            }
            return;
        }

        bool playerIsFar = horizontalDistance > loseRange || verticalDistance > verticalDetectionRange;
        if (playerIsFar)
        {
            StopChasingAndReturn();
        }

    }

    private void Chase()
    {
        if (player == null)
        {
            StopChasingAndReturn();
            return;
        }
        float distanceToPlayerX = Mathf.Abs(player.position.x - transform.position.x);

        if (distanceToPlayerX <= attackStopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }
        float directionToPlayerX = player.position.x - transform.position.x;
        int chaseDirection = directionToPlayerX > 0f ? 1 : -1;

        if (!CanMoveInDirection(chaseDirection))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            StopChasingAndReturn();
            return;
        }

        rb.linearVelocity = new Vector2(moveSpeed * chaseDirection, rb.linearVelocity.y);
        animator.SetBool("isRunning", true);
        FlipSprite();
    }

    private bool CanMoveInDirection(int direction)
    {
        Vector2 wallCheckOrigin = bodyCollider.bounds.center;

        float wallRayDistance = bodyCollider.bounds.extents.x + wallCheckDistance;

        RaycastHit2D wallHit = Physics2D.Raycast(
            wallCheckOrigin,
            Vector2.right * direction,
            wallRayDistance,
            groundLayer
        );

        Vector2 groundCheckOrigin = new Vector2(
            bodyCollider.bounds.center.x +
                direction * (bodyCollider.bounds.extents.x + wallCheckDistance),
            bodyCollider.bounds.min.y + 0.05f
        );

        RaycastHit2D groundHit = Physics2D.Raycast(
            groundCheckOrigin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        bool hasWallAhead = wallHit.collider != null;
        bool hasGroundAhead = groundHit.collider != null;

        return !hasWallAhead && hasGroundAhead;
    }

    private void StopChasingAndReturn()
    {
        isChasing = false;

        returnPositionX = Mathf.Clamp(
            transform.position.x,
            leftPosition,
            rightPosition
        );

        isReturning =
            transform.position.x < leftPosition ||
            transform.position.x > rightPosition;
    }

    private void ReturnToPatrol()
    {
        float distanceToReturnPosition =
            returnPositionX - transform.position.x;

        if (Mathf.Abs(distanceToReturnPosition) <= 0.05f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isReturning = false;

            moveDirection = returnPositionX <= leftPosition ? 1 : -1;
            StartRunning();
            return;
        }

        int returnDirection = distanceToReturnPosition > 0f ? 1 : -1;

        if (!CanMoveInDirection(returnDirection))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }

        rb.linearVelocity = new Vector2(
            moveSpeed * returnDirection,
            rb.linearVelocity.y
        );

        animator.SetBool("isRunning", true);
        FlipSprite();
    }


    private void Run()
    {
        rb.linearVelocity = new Vector2(moveSpeed * moveDirection, rb.linearVelocity.y);
        if (!CanMoveInDirection(moveDirection))
        {
            StartIdle();
            return;
        }

        FlipSprite();
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
