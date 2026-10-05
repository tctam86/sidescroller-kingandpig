using UnityEngine;
public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private BossConfig bossConfig;
    Animator animator;
    private int currentHealth;
    private bool isDead;
    private Rigidbody2D rb;
    [SerializeField] private BossHealthUI healthBarUI;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => bossConfig != null ? bossConfig.maximumHeart : 0;
    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (bossConfig == null)
        {
            Debug.LogError("BossHealth doesn't include boss config.", this);
            enabled = false;
            return;
        }



    }
    private void Start()
    {
        currentHealth = bossConfig.maximumHeart;
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth, bossConfig.maximumHeart);
        }

    }

    public void TakeDamage(int damage, float attackerPositionX)
    {
        if (!enabled || isDead || damage <= 0) { return; }

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);
        
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth, bossConfig.maximumHeart);
        }

        if (currentHealth == 0) { Die(); return; }

        animator.SetTrigger("Hit");
    }

    private void Die()
    {
        if (isDead) { return; }
        isDead = true;
        foreach (Collider2D bossCollider in GetComponentsInChildren<Collider2D>())
        {
            bossCollider.enabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        animator.ResetTrigger("Hit");
        animator.SetTrigger("Dead");

        DropDiamonds();
    }

    private void DropDiamonds()
    {
        if (bossConfig.droppedDiamondPrefab == null) { return; }
        int dropAmount = bossConfig.diamondDrop;

        for (int i = 0; i < dropAmount; i++)
        {
            //Diamond drop
            Vector3 spawnPosition = transform.position + Vector3.up * 0.4f;
            GameObject droppedDiamond = Instantiate(bossConfig.droppedDiamondPrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D diamondRigidbody = droppedDiamond.GetComponent<Rigidbody2D>();
            if (diamondRigidbody == null)
            {
                continue;
            }

            float horizontalForce = Random.Range(
            -bossConfig.horizontalDropForce,
                bossConfig.horizontalDropForce
            );

            float verticalForce = Random.Range(
                bossConfig.verticalDropForceRange.x,
                bossConfig.verticalDropForceRange.y
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
