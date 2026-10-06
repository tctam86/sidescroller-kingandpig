using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [SerializeField] private BossConfig bossConfig;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private float groundCheckDistance = 0.15f;

    [SerializeField] private float attackStopDistance = 0.5f;



    [SerializeField] private Transform player;

    private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D bodyColiider;
    private BossHealth bossHealth;

    private Vector3 startingScale;

    private int moveDirection;
    private float movementPauseEndTime;

    private bool isDead;

    public void PauseMovement(float duration)
    {
        movementPauseEndTime = Mathf.Max(movementPauseEndTime, Time.time + duration);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyColiider = GetComponent<CapsuleCollider2D>();
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

        Chase();
    }


    private void Update()
    {

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

        Run();


    }

}
