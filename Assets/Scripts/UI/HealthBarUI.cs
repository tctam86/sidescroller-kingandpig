using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private HeartUI[] hearts;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHearts;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHearts;
        }
    }

    private void Start()
    {
        UpdateHearts(playerHealth.CurrentHealthUnits);
    }

    private void UpdateHearts(int units)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (units > i)
            {
                hearts[i].Show();
            }
            else
            {
                hearts[i].PlayHitAndHide();
            }
        }
    }
}
