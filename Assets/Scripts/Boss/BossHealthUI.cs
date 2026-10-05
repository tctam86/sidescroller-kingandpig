using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (fillImage == null || maxHealth <= 0) { return; }
        fillImage.fillAmount = Mathf.Clamp01((float)currentHealth / maxHealth);
    }
}

