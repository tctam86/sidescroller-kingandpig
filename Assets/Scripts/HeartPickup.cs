using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    private bool wasCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollet(other);
    }

    private void TryCollet(Collider2D other)
    {
        if (wasCollected)
        {
            return;
        }
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }
        if (!playerHealth.Heal(healAmount))
        {
            return;
        }
        Destroy(gameObject);
        wasCollected = true;

    }
}
