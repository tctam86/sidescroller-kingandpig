using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{

    [Header("Player Config")]
    [SerializeField] private float attackCooldown = 0.5f;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;




    private Animator animator;

    private float attackNextTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    private void OnAttack(InputValue value)
    {
        if (!value.isPressed || Time.time < attackNextTime)
        {
            return;
        }

        attackNextTime = Time.time + attackCooldown;
        animator.SetTrigger("Attack");
        DealDamage();
    }

    private void DealDamage()
    {
        if (attackPoint == null)
        {
            return;
        }
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemyCollider in enemiesHit)
        {
            IDamageable damageable = enemyCollider.GetComponentInParent<IDamageable>();

            damageable?.TakeDamage(attackDamage, transform.position.x);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
