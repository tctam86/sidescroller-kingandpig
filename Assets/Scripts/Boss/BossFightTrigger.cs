using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] private BossMovement bossMovement;
    [SerializeField] private BossAttack bossAttack;

    [SerializeField] private GameObject bossHealthBar;

    private bool hasStarted;

    private void Start()
    {
        bossMovement.enabled = false;
        bossAttack.enabled = false;
        bossHealthBar.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasStarted)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            hasStarted = true;
            bossMovement.enabled = true;
            bossAttack.enabled = true;
            bossHealthBar.SetActive(true);
        }
    }
}
