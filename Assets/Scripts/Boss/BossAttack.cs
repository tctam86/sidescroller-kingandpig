using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [SerializeField] private BossConfig bossConfig;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.75f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private LayerMask playerLayer;

    private Animator animator;
    private BossHealth bossHealth;
    private BossMovement bossMovement;
    private float nextAttackTime;
    private bool attackResolved;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        bossMovement = GetComponent<BossMovement>();

        if (bossConfig == null)
        {
            Debug.LogError("BossAttack doesn't include boss config.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (!enabled || bossHealth == null || bossHealth.IsDead || Time.time < nextAttackTime)
        {
            return;
        }

        if (bossMovement != null && bossMovement.IsPhase2) { return; }

        Vector2 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D playerCollider = Physics2D.OverlapCircle(attackPosition, attackRange, playerLayer);

        if (playerCollider == null)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        attackResolved = false;
        bossMovement?.PauseMovement(attackCooldown);
        animator.SetTrigger("Attack");
    }

    public void DealAttackDamage()
    {
        if (!enabled || attackResolved || bossHealth == null || bossHealth.IsDead)
        {
            return;
        }

        attackResolved = true;
        Vector2 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D playerCollider = Physics2D.OverlapCircle(attackPosition, attackRange, playerLayer);

        if (playerCollider == null)
        {
            return;
        }

        IDamageable damageable = playerCollider.GetComponentInParent<IDamageable>();
        damageable?.TakeDamage(bossConfig.attackDamageUnits, transform.position.x);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}