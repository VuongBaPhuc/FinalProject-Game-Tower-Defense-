using System;
using UnityEngine;

public class CastleUnit : StructureUnit
{
    public event Action<CastleUnit> WoodChanged;

    public int StoredWood { get; private set; }

    public void DepositWood(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError($"Deposit amount must be greater than zero. Received {amount}.", this);
            return;
        }

        StoredWood += amount;
        WoodChanged?.Invoke(this);
        Debug.Log($"{name} received {amount} wood. Total stored wood: {StoredWood}.", this);
    }
}
