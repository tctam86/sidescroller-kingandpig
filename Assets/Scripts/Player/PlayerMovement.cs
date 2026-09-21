using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpForce = 5f;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private CapsuleCollider2D bodyCollider;
    private BoxCollider2D feetCollider;
    private bool jumpRequest;

    [Header("Knockback")]
    [SerializeField] private float knockbackSpeed = 2f;
    [SerializeField] private float knockbackDuration = 0.1f;
    private bool isKnockback;
    private float knockbackEndTime;
    private float knockbackDirection;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        feetCollider = GetComponent<BoxCollider2D>();
    }



    private void FixedUpdate()
    {
        if (isKnockback)
        {
            if (Time.time < knockbackEndTime)
            {
                rb.linearVelocity = new Vector2(knockbackDirection * knockbackSpeed, rb.linearVelocity.y);
                return;
            }

            isKnockback = false;
        }

        Run();
        FlipSprite();
        HandleJump();

    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void HandleJump()
    {
        if (!jumpRequest)
        {
            return;

        }
        jumpRequest = false;

        if (!IsGrounded())
        {
            return;
        }
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }


    private void UpdateAnimation()
    {
        bool isRunning = Mathf.Abs(moveInput.x) > Mathf.Epsilon;
        animator.SetBool("isRunning", isRunning);


        bool grounded = IsGrounded();
        float verticalspeed = rb.linearVelocity.y;
        bool isJumping = !grounded && verticalspeed > 0.1f;
        bool isFalling = !grounded && verticalspeed < -0.1f;

        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isGrounded", grounded);
    }

    private void Run()
    {
        Vector2 playerVelocity = new Vector2(moveSpeed * moveInput.x, rb.linearVelocity.y);
        rb.linearVelocity = playerVelocity;
        bool hasHorizontalSpeed = Mathf.Abs(rb.linearVelocity.x) > Mathf.Epsilon;

        animator.SetBool("isRunning", hasHorizontalSpeed);
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FlipSprite()
    {
        bool hasHorizontalSpeed = Mathf.Abs(rb.linearVelocity.x) > Mathf.Epsilon;
        if (!hasHorizontalSpeed)
        {
            return;
        }

        float direction = Mathf.Sign(rb.linearVelocity.x);
        if (Mathf.Sign(transform.localScale.x) == direction)
        {
            return;
        }


        Vector3 colliderCenterBeforeFlip = transform.TransformPoint(bodyCollider.offset);

        transform.localScale = new Vector3(
            direction,
            transform.localScale.y,
            transform.localScale.z
        );

        Vector3 colliderCenterAfterFlip = transform.TransformPoint(bodyCollider.offset);
        transform.position += colliderCenterBeforeFlip - colliderCenterAfterFlip;
    }

    private bool IsGrounded()
    {
        return feetCollider.IsTouchingLayers(groundLayer);
    }


    private void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpRequest = true;
        }
    }


    public void ApplyKnockback(float direction)
    {
        knockbackDirection = Mathf.Sign(direction);

        isKnockback = true;

        knockbackEndTime = Time.time + knockbackDuration;

        jumpRequest = false;

        rb.linearVelocity = new Vector2(direction * knockbackSpeed, rb.linearVelocity.y);

    }
}
