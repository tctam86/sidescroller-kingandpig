using UnityEngine;
using UnityEngine.UI;

public class DiamondScoreUI : MonoBehaviour
{
    [SerializeField] private Image tensImage;
    [SerializeField] private Image onesImage;
    [SerializeField] private Sprite[] numberSprites;
    void Start()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogError("Cannot find GameSession object in current scene.", this);
            return;
        }
        GameSession.Instance.OnDiamondCountChanged += UpdateDisplay;
        UpdateDisplay(GameSession.Instance.DiamondCount);
    }

    private void OnDestroy()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnDiamondCountChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(int diamondCount)
    {
        if (numberSprites == null || numberSprites.Length < 10)
        {
            Debug.LogWarning("DiamondScoreUI is not enough the number of necessary sprites", this);
            return;
        }
        int displayValue = Mathf.Clamp(diamondCount, 0, 99);
        int onesDigit = displayValue % 10;
        int tensDigit = (displayValue / 10) % 10;
        onesImage.sprite = numberSprites[onesDigit];
        bool showTensDigit = displayValue >= 10;
        tensImage.gameObject.SetActive(showTensDigit);
        if (showTensDigit)
        {
            tensImage.sprite = numberSprites[tensDigit];
        }
    }
}
