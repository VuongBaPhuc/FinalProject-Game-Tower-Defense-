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
        if (unit != null && unit.Actions.Count > buttons.Length)
        {
            Debug.LogWarning(
                $"ActionBar has {buttons.Length} slots, but '{unit.name}' has {unit.Actions.Count} actions.",
                this);
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            ActionButton actionButton = button.GetComponent<ActionButton>();
            if (actionButton == null)
            {
                actionButton = button.gameObject.AddComponent<ActionButton>();
            }

            UnitActionSO action = unit != null && i < unit.Actions.Count
                ? unit.Actions[i]
                : null;
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

}
