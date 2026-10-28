using UnityEngine;

public class TreeResourceNode : MonoBehaviour
{
    [SerializeField, Min(1)] private int m_WoodRemaining = 10;

    public bool HasResources => m_WoodRemaining > 0;
    public int WoodRemaining => m_WoodRemaining;

    public int Gather(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError($"Gather amount must be greater than zero. Received {amount}.", this);
            return 0;
        }

        if (!HasResources)
        {
            return 0;
        }

        int gathered = Mathf.Min(amount, m_WoodRemaining);
        m_WoodRemaining -= gathered;

        if (m_WoodRemaining == 0)
        {
            Destroy(gameObject);
        }

        return gathered;
    }
}
