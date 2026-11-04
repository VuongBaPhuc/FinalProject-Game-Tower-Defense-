using UnityEngine;
using UnityEngine.EventSystems;

// Provides mouse and touch panning and zooming for the orthographic game camera.
public class CameraController2D : MonoBehaviour
{
    [SerializeField, Min(1f)] private float m_MinZoom = 3f;
    [SerializeField, Min(1f)] private float m_MaxZoom = 12f;
    [SerializeField, Min(0.01f)] private float m_MouseZoomSpeed = 1f;
    [SerializeField, Min(0.001f)] private float m_PinchZoomSpeed = 0.01f;

    private Camera m_Camera;
    private bool m_IsPanning;
    private int m_PanButton;
    private Vector2 m_PreviousPointerPosition;
    private Vector2 m_LeftDragStartPosition;
    private bool m_IsLeftButtonDown;
    private Vector2 m_TouchStartPosition;
    private float m_PreviousPinchDistance;

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        if (m_Camera == null)
        {
            Debug.LogError("CameraController2D must be attached to a Camera.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        HandleTouchInput();
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0f && !IsPointerOverUI(-1))
        {
            SetZoom(m_Camera.orthographicSize - Input.mouseScrollDelta.y * m_MouseZoomSpeed);
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_LeftDragStartPosition = Input.mousePosition;
            m_PreviousPointerPosition = Input.mousePosition;
            m_IsLeftButtonDown = !IsPointerOverUI(-1);
        }

        if (m_IsLeftButtonDown && Input.GetMouseButton(0))
        {
            Vector2 leftDragPosition = Input.mousePosition;
            if (!m_IsPanning &&
                Vector2.Distance(m_LeftDragStartPosition, leftDragPosition) >= 10f)
            {
                m_IsPanning = true;
            }

            if (m_IsPanning)
            {
                Vector3 leftDragPreviousWorldPosition = ScreenToWorld(m_PreviousPointerPosition);
                Vector3 leftDragCurrentWorldPosition = ScreenToWorld(leftDragPosition);
                transform.position += leftDragPreviousWorldPosition - leftDragCurrentWorldPosition;
                m_PreviousPointerPosition = leftDragPosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_IsLeftButtonDown = false;
            m_IsPanning = false;
        }

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            if (!IsPointerOverUI(-1))
            {
                m_PanButton = Input.GetMouseButtonDown(1) ? 1 : 2;
                m_PreviousPointerPosition = Input.mousePosition;
                m_IsPanning = true;
            }
        }

        if (!m_IsPanning)
        {
            return;
        }

        if (!Input.GetMouseButton(m_PanButton))
        {
            m_IsPanning = false;
            return;
        }

        Vector2 currentPosition = Input.mousePosition;
        Vector3 previousWorldPosition = ScreenToWorld(m_PreviousPointerPosition);
        Vector3 currentWorldPosition = ScreenToWorld(currentPosition);
        transform.position += previousWorldPosition - currentWorldPosition;
        m_PreviousPointerPosition = currentPosition;
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            m_PreviousPinchDistance = 0f;
            m_IsPanning = false;
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                m_TouchStartPosition = touch.position;
                m_PreviousPointerPosition = touch.position;
                m_IsPanning = !IsPointerOverUI(touch.fingerId);
            }
            else if (m_IsPanning &&
                     Vector2.Distance(m_TouchStartPosition, touch.position) >= 10f)
            {
                Vector3 previousWorldPosition = ScreenToWorld(m_PreviousPointerPosition);
                Vector3 currentWorldPosition = ScreenToWorld(touch.position);
                transform.position += previousWorldPosition - currentWorldPosition;
                m_PreviousPointerPosition = touch.position;
            }

            m_PreviousPinchDistance = 0f;
            return;
        }

        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);
        if (IsPointerOverUI(firstTouch.fingerId) || IsPointerOverUI(secondTouch.fingerId))
        {
            m_PreviousPinchDistance = 0f;
            return;
        }

        float currentDistance = Vector2.Distance(firstTouch.position, secondTouch.position);
        if (m_PreviousPinchDistance > 0f)
        {
            SetZoom(m_Camera.orthographicSize -
                (currentDistance - m_PreviousPinchDistance) * m_PinchZoomSpeed);
        }

        m_PreviousPinchDistance = currentDistance;
        m_IsPanning = false;
    }

    private void SetZoom(float zoom)
    {
        m_Camera.orthographicSize = Mathf.Clamp(zoom, m_MinZoom, m_MaxZoom);
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        return m_Camera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            Mathf.Abs(m_Camera.transform.position.z)));
    }

    private static bool IsPointerOverUI(int pointerId)
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}
