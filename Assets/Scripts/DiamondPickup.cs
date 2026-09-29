using UnityEngine;

public class DiamondPickup : MonoBehaviour
{
    [SerializeField] private int diamondValue = 1;
    private bool wasCollected = false;
    [SerializeField] private float pickupDelay = 0f;
    private float pickupAvailableTime;

    private void Awake()
    {
        pickupAvailableTime = Time.time + pickupDelay;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollect(collision.gameObject);
    }

    private void TryCollect(GameObject other)
    {
        if (Time.time < pickupAvailableTime) { return; }
        if (wasCollected) { return; }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (GameSession.Instance == null)
        {
            Debug.LogError(
               "Cannot find GameSession. Collected Diamond cannot store.",
               this
           );

            return;
        }

        wasCollected = true;
        GameSession.Instance.AddDiamond(diamondValue);
        Destroy(gameObject);
    }
}
