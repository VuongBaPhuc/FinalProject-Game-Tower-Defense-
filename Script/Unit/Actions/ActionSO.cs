using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitActionEvent : UnityEvent<Unit>
{
}

[CreateAssetMenu(fileName = "UnitAction", menuName = "Units/Action")]
public class UnitActionSO : ScriptableObject
{
    [SerializeField] private string m_DisplayName;
    [SerializeField] private Sprite m_Icon;
    [SerializeField] private UnitActionEvent m_OnExecute = new();

    public string DisplayName => m_DisplayName;
    public Sprite Icon => m_Icon;

    public virtual void Execute(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError($"Cannot execute action '{m_DisplayName}' without a unit.", this);
            return;
        }

        m_OnExecute.Invoke(unit);
    }
}
