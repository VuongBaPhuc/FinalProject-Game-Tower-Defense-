using System.Collections;
using UnityEngine;

public class PointToClick : MonoBehaviour
{
    [SerializeField] private float m_Duration = 0.5f;
    [SerializeField] private float m_MaxScale = 1.5f;
    [SerializeField] private AnimationCurve m_ScaleCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f));

    private void Start()
    {
        StartCoroutine(PlayScaleAnimation());
    }

    private IEnumerator PlayScaleAnimation()
    {
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < m_Duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / m_Duration);
            float curveValue = m_ScaleCurve.Evaluate(progress);

            transform.localScale = Vector3.Lerp(
                initialScale,
                initialScale * m_MaxScale,
                curveValue);

            yield return null;
        }

        Destroy(gameObject);
    }
}
