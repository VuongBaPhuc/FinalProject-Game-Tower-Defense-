using UnityEngine;

public static class DamageNumberPopup
{
    public static void Show(Transform target, float damage)
    {
        if (target == null)
        {
            Debug.LogError("A target is required to show a damage number.");
            return;
        }

        if (damage <= 0f)
        {
            Debug.LogError($"Damage number must be greater than zero. Received {damage}.", target);
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            Debug.LogError("Could not load Unity's built-in LegacyRuntime.ttf font.", target);
            return;
        }

        GameObject popup = new("DamageNumber");
        popup.transform.position = target.position + Vector3.up * 1.5f;

        TextMesh textMesh = popup.AddComponent<TextMesh>();
        textMesh.font = font;
        textMesh.text = $"-{damage:0.#}";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.1f;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.color = new Color(1f, 0.2f, 0.2f, 1f);

        popup.GetComponent<MeshRenderer>().sortingOrder = 100;
        Object.Destroy(popup, 0.8f);
    }
}
