using UnityEngine;
using System;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    [SerializeField] private int diamondCount = 0;
    public int DiamondCount => diamondCount;
    public event Action<int> OnDiamondCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //Set this object to main Gamsession
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void AddDiamond(int amount)
    {
        if (amount <= 0) { return; }
        diamondCount += amount;
        OnDiamondCountChanged?.Invoke(diamondCount);
    }
    public void ResetDiamond()
    {
        diamondCount = 0;
        OnDiamondCountChanged?.Invoke(diamondCount);
    }
    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

}
