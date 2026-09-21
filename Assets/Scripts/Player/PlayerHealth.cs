using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private const int UnitsPerHeart = 2;

    [SerializeField] private int maximumHeart = 3;

    [SerializeField] private float invulnerabilityDuration = 0.75f;

    public int CurrentHealthUnits { get; private set; }

    public int MaximumHealthUnits => maximumHeart * UnitsPerHeart;

    private bool isDead;
    private float nextDamageTime;
    Animator animator;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        CurrentHealthUnits = MaximumHealthUnits;
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(int damage, float attackerPositionX)
    {
        if (isDead || damage <= 0 || Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTime = Time.time + invulnerabilityDuration;
        animator.SetTrigger("Hit");

        float knockbackDirection = transform.position.x - attackerPositionX;
        playerMovement.ApplyKnockback(knockbackDirection);

        CurrentHealthUnits = Mathf.Max(CurrentHealthUnits - damage, 0);

        if (CurrentHealthUnits == 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Dead");
    }
}
