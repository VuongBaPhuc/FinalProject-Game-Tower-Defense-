using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour
{
    // Cached UI components and click callback for the currently assigned action.
    private Button m_Button;
    private Image m_Icon;
    private UnityAction m_ClickListener;

    // Assigns the action icon and replaces the previous click callback.
    public void Initialize(UnitActionSO action, Unit unit)
    {
        m_Button = GetComponent<Button>();
        m_Icon = FindIcon();

        if (action == null || unit == null)
        {
            Debug.LogError("ActionButton cannot initialize without an action and a selected unit.", this);
            Hide();
            return;
        }

        if (m_Button == null || m_Icon == null)
        {
            Debug.LogError("ActionButton requires a Button and an icon Image.", this);
            return;
        }

        if (action.Icon == null)
        {
            Debug.LogError($"Action '{action.DisplayName}' requires an icon Sprite.", action);
            return;
        }

        if (m_ClickListener != null)
        {
            m_Button.onClick.RemoveListener(m_ClickListener);
        }

        m_Icon.sprite = action.Icon;
        m_ClickListener = () =>
        {
            if (action == null || unit == null || !isActiveAndEnabled)
            {
                Hide();
                return;
            }

            action.Execute(unit);
        };
        m_Button.onClick.AddListener(m_ClickListener);
        gameObject.SetActive(true);
    }

    // Hides this button when the selected unit does not support its action.
    public void Hide()
    {
        if (m_Button == null)
        {
            m_Button = GetComponent<Button>();
        }

        if (m_ClickListener != null)
        {
            m_Button.onClick.RemoveListener(m_ClickListener);
            m_ClickListener = null;
        }

        gameObject.SetActive(false);
    }

    // Finds the button's child icon, falling back to an Image on the button itself.
    private Image FindIcon()
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject != gameObject)
            {
                return image;
            }
        }

        return GetComponent<Image>();
    }
}
