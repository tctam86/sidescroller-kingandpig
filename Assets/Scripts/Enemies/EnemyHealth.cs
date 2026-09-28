using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyConfig enemyConfig;
    Animator animator;
    private EnemyMovement enemyMovement;
    private int currentHealth;
    private bool isDead;
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
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);

        animator.SetTrigger("Hit");

        float knockbackDirection = transform.position.x - attackerPositionX;
        enemyMovement.ApplyKnockback(knockbackDirection);

        if (currentHealth == 0)
        {
            Die();
        }

    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;
        DropDiamonds();
        Destroy(gameObject);
    }

    private void DropDiamonds()
    {
        if (enemyConfig.droppedDiamondPrefab == null)
        {
            return;
        }

        int dropAmount = Random.Range(enemyConfig.minimumDiamondDrop, enemyConfig.maximumDiamondDrop + 1);

        for (int i = 0; i < dropAmount; i++)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 0.4f;
            GameObject droppedDiamond = Instantiate(enemyConfig.droppedDiamondPrefab,
            spawnPosition,
            Quaternion.identity);

            Rigidbody2D diamondRigidbody = droppedDiamond.GetComponent<Rigidbody2D>();

            if (diamondRigidbody == null)
            {
                continue;
            }

            float horizontalForce = Random.Range(
            -enemyConfig.horizontalDropForce,
            enemyConfig.horizontalDropForce
        );
            float verticalForce = Random.Range(
                enemyConfig.verticalDropForceRange.x,
                enemyConfig.verticalDropForceRange.y
            );

            Vector2 dropForce = new Vector2(
            horizontalForce,
            verticalForce
        );
            diamondRigidbody.AddForce(
             dropForce,
             ForceMode2D.Impulse
         );
        }
    }
}
