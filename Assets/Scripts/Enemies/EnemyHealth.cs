using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyConfig enemyConfig;
    Animator animator;
    private EnemyMovement enemyMovement;
    private int currentHealth;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyMovement = GetComponent<EnemyMovement>();
    }
    private void Start()
    {
        currentHealth = enemyConfig.maximumHeart;
    }

    public void TakeDamage(int damage, float attackerPositionX)
    {
        if(damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth,0);

        animator.SetTrigger("Hit");

        float knockbackDirection = transform.position.x - attackerPositionX;
        enemyMovement.ApplyKnockback(knockbackDirection);

        if(currentHealth == 0)
        {
            Die();
        }
        
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
