
using NUnit.Framework.Constraints;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [SerializeField] private BossConfig bossConfig;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private float groundCheckDistance = 0.15f;

    [SerializeField] private float attackStopDistance = 0.5f;

    [SerializeField] private Transform phase2Position;

    [SerializeField] private Transform player;

    private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D bodyCollider;
    private BossHealth bossHealth;



    private Vector3 startingScale;

    private int moveDirection;
    private float movementPauseEndTime;

    private bool isDead;

    private bool isPhase2;

    public bool IsPhase2 => isPhase2;

    public void PauseMovement(float duration)
    {
        movementPauseEndTime = Mathf.Max(movementPauseEndTime, Time.time + duration);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
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

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        bossHealth = GetComponent<BossHealth>();


        startingScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        if (bossHealth != null && bossHealth.IsDead)
        {
            isDead = true;
        }

        if (isDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }
        PhaseChanging();

        if (isPhase2)
        {
            MoveToPhase2Position();
            return;
        }

        Chase();
    }


    public void MoveToPhase2Position()
    {
        if (phase2Position == null || Time.time < movementPauseEndTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }
        float distanceX = phase2Position.position.x - transform.position.x;
        
        if (Mathf.Abs(distanceX) <= 0.1f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }

        moveDirection = distanceX > 0f ? 1 : -1;
        if (!CanMoveInDirection(moveDirection))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }
        Run();
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

    private void Run()
    {
        rb.linearVelocity = new Vector2(bossConfig.moveSpeed * moveDirection, rb.linearVelocity.y);

        FlipSprite();
        animator.SetBool("isRunning", true);

    }

    private void PhaseChanging()
    {
        if (isPhase2 || bossHealth == null) { return; }
        if (bossHealth.CurrentHealth <= bossHealth.MaxHealth * 0.5f)
        {
            isPhase2 = true;
        }
    }

    private void Chase()
    {


        if (Time.time < movementPauseEndTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }

        if (player == null) { return; }



        float distancetoPlayerX = Mathf.Abs(player.position.x - transform.position.x);
        if (distancetoPlayerX <= attackStopDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }
        if (player.position.x > transform.position.x)
        {
            moveDirection = 1;
        }
        else
        {
            moveDirection = -1;
        }

        if (!CanMoveInDirection(moveDirection))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }

        Run();


    }

}
