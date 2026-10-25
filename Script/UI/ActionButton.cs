using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour
{
    private Button m_Button;
    private Image m_Icon;
    private UnityAction m_ClickListener;

    public void Initialize(UnitActionSO action, Unit unit)
    {
        m_Button = GetComponent<Button>();
        m_Icon = FindIcon();

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
        m_ClickListener = () => action.Execute(unit);
        m_Button.onClick.AddListener(m_ClickListener);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (m_Button == null)
        {
            m_Button = GetComponent<Button>();
        }

        gameObject.SetActive(false);
    }

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
