using System;
using UnityEngine;

public class CastleUnit : StructureUnit
{
    [SerializeField, Min(0)] private int m_StartingFood = 25;

    // Notifies the resource HUD after wood is added to this castle.
    public event Action<CastleUnit> WoodChanged;
    // Notifies the resource HUD after food is added to this castle.
    public event Action<CastleUnit> FoodChanged;

    // Total wood stored by this castle.
    public int StoredWood { get; private set; }
    // Total food stored by this castle.
    public int StoredFood { get; private set; }
    private WorldHealthBar m_HealthBar;

    // Initializes starting food and ensures the Castle has a visible health bar.
    protected override void Awake()
    {
        base.Awake();
        StoredFood = m_StartingFood;
        m_HealthBar = GetComponent<WorldHealthBar>();
        if (m_HealthBar == null)
        {
            m_HealthBar = gameObject.AddComponent<WorldHealthBar>();
        }
    }

    // Keeps the Castle's world-space health bar synchronized with its health.
    private void LateUpdate()
    {
        m_HealthBar?.SetHealth(CurrentHealth, MaxHealth);
    }

    // Ends the game when the player's Castle is destroyed.
    protected override void Die()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("The Castle was destroyed, but no GameManager can show the defeat screen.", this);
        }
        else
        {
            gameManager.EndGame(GameManager.GameResult.Loss);
        }

        base.Die();
    }

    // Adds gathered wood and notifies listeners that the total changed.
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

    // Adds gathered food and notifies listeners that the total changed.
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
