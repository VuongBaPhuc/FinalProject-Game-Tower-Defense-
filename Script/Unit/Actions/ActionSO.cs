using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
// UnityEvent wrapper that passes the acting unit to configured Inspector listeners.
public class UnitActionEvent : UnityEvent<Unit>
{
}

[CreateAssetMenu(fileName = "UnitAction", menuName = "Units/Action")]
// Base ScriptableObject for reusable actions shown in a unit's action bar.
public class UnitActionSO : ScriptableObject
{
    // Label and icon used to identify this action in the UI.
    [SerializeField] private string m_DisplayName;
    [SerializeField] private Sprite m_Icon;
    // Optional Inspector-configured callbacks invoked when the action is selected.
    [SerializeField] private UnitActionEvent m_OnExecute = new();

    public string DisplayName => m_DisplayName;
    public Sprite Icon => m_Icon;

    // Validates the acting unit and invokes the configured action callbacks.
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
