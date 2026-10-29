using System;
using UnityEngine;

public class CastleUnit : StructureUnit
{
    public event Action<CastleUnit> WoodChanged;
    public event Action<CastleUnit> FoodChanged;

    public int StoredWood { get; private set; }
    public int StoredFood { get; private set; }

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

    public void DepositFood(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError($"Food deposit amount must be greater than zero. Received {amount}.", this);
            return;
        }

        StoredFood += amount;
        FoodChanged?.Invoke(this);
        Debug.Log($"{name} received {amount} food. Total stored food: {StoredFood}.", this);
    }
}
