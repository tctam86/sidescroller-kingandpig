using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
public class PlayerHealth : MonoBehaviour, IDamageable
{
    private const int UnitsPerHeart = 1;

    [SerializeField] private int maximumHeart = 3;

    [SerializeField] private float invulnerabilityDuration = 0.75f;

    public int CurrentHealthUnits { get; private set; }

    public int MaximumHealthUnits => maximumHeart * UnitsPerHeart;

    public event System.Action<int> OnHealthChanged;

    private bool isDead;
    private float nextDamageTime;
    Animator animator;
    private PlayerMovement playerMovement;

    [SerializeField] private string deathStateName = "Death";

    private PlayerInput playerInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        CurrentHealthUnits = MaximumHealthUnits;
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        
    }

    public void TakeDamage(int damage, float attackerPositionX)
    {
        if (isDead || damage <= 0 || Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTime = Time.time + invulnerabilityDuration;
        animator.SetTrigger("Hit");

        GetComponent<CinemachineImpulseSource>()?.GenerateImpulse(0.35f);

        float knockbackDirection = transform.position.x - attackerPositionX;
        playerMovement.ApplyKnockback(knockbackDirection);

        CurrentHealthUnits = Mathf.Max(CurrentHealthUnits - damage, 0);
        OnHealthChanged?.Invoke(CurrentHealthUnits);

        if (CurrentHealthUnits == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Dead");
        LockPlayerInput();
        StartCoroutine(DeathSequence());
    }

    private void LockPlayerInput()
    {
        //Lock player Input
        playerInput.DeactivateInput();
        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName));
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
