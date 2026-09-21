using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyConfig enemyConfig;

    [SerializeField] private Transform attackPoint;

    // Radius used to detect the Player.
    [SerializeField] private float attackRange = 0.5f;

    [SerializeField] private float attackCooldown = 1f;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private float pauseAfterAttack = 0.5f;

    private EnemyMovement enemyMovement;

    private Animator animator;
    private float nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (Time.time < nextAttackTime || attackPoint == null)
        {
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        if (playerCollider == null)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        animator.SetTrigger("Attack");
    }

    public void DealAttackDamage()
    {
        if (attackPoint == null)
        {
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        if (playerCollider == null)
        {
            return;
        }

        IDamageable damageable =
            playerCollider.GetComponentInParent<IDamageable>();

        damageable?.TakeDamage(
            enemyConfig.attackDamageUnits,
            transform.position.x
        );
        enemyMovement.PauseMovement(pauseAfterAttack);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
