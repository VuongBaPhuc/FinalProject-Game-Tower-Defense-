using UnityEngine;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    // Container holding the buttons that represent possible unit actions.
    [SerializeField] private Transform m_ActionContainer;

    // Configures buttons to match the selected unit and hides unavailable actions.
    public void Show(Unit unit)
    {
        if (m_ActionContainer == null)
        {
            Debug.LogError("ActionBar requires the UI Action Container from the HUD.", this);
            return;
        }

        Button[] buttons = m_ActionContainer.GetComponentsInChildren<Button>(true);
        if (buttons.Length == 0)
        {
            Debug.LogError("ActionBar container has no child Buttons.", m_ActionContainer);
            return;
        }

        m_ActionContainer.gameObject.SetActive(unit != null && unit.Actions.Count > 0);

        foreach (Button button in buttons)
        {
            ActionButton actionButton = button.GetComponent<ActionButton>();
            if (actionButton == null)
            {
                actionButton = button.gameObject.AddComponent<ActionButton>();
            }

            UnitActionSO action = FindActionForButton(unit, button);
            if (action == null)
            {
                actionButton.Hide();
                continue;
            }

            actionButton.Initialize(action, unit);
        }
    }

    // Refreshes resource-based enabled states while this action bar is visible.
    public void RefreshActionAvailability(GameManager gameManager)
    {
        if (m_ActionContainer == null || !m_ActionContainer.gameObject.activeInHierarchy)
        {
            return;
        }

        foreach (ActionButton actionButton in m_ActionContainer.GetComponentsInChildren<ActionButton>(true))
        {
            actionButton.RefreshAvailability(gameManager);
        }
    }

    // Matches an action to a button using the button's display name.
    private UnitActionSO FindActionForButton(Unit unit, Button button)
    {
        if (unit == null)
        {
            return null;
        }

        string buttonName = button.name;
        int openParenthesis = buttonName.LastIndexOf('(');
        int closeParenthesis = buttonName.LastIndexOf(')');
        if (openParenthesis >= 0 && closeParenthesis > openParenthesis)
        {
            buttonName = buttonName.Substring(
                openParenthesis + 1,
                closeParenthesis - openParenthesis - 1);
        }

        foreach (UnitActionSO action in unit.Actions)
        {
            if (action != null &&
                string.Equals(action.DisplayName, buttonName, System.StringComparison.OrdinalIgnoreCase))
            {
                return action;
            }
        }

        return null;
    }
}
