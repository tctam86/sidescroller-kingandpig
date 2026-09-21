using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private EnemyConfig enemyConfig;

    [SerializeField] private LayerMask playerLayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) == 0)
        {
            return;
        }

        IDamageable damageable =
            collision.collider.GetComponentInParent<IDamageable>();

        damageable?.TakeDamage(
            enemyConfig.contactDamageUnits,
            transform.position.x
        );
    }
}
