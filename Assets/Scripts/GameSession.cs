using UnityEngine;
using System;
using NUnit.Framework;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    [SerializeField] private int diamondCount = 0;
    public int DiamondCount => diamondCount;
    public event Action<int> OnDiamondCountChanged;

    //Heart 
    private int savedHealthUnits;

    public int SavedHealthUnits => savedHealthUnits;

    public bool HasSavedHealth { get; private set; }


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

    public void SavePlayerHealth(int healUnits)
    {
        savedHealthUnits = healUnits;
        HasSavedHealth = true;
    }

    public void ResetSavedHealth()
    {
        HasSavedHealth = false;
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
        if (Instance == this)
        {
            Instance = null;
        }
    }

}
