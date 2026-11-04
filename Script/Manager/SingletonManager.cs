


using UnityEngine;

// Provides duplicate prevention and lazy lookup for manager components.
public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    // Destroys this GameObject when another instance of the same manager exists.
    protected virtual void Awake()
    {
        // Use the non-deprecated overload and include inactive objects
        T[] managers = FindObjectsByType<T>(FindObjectsInactive.Include);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Finds a manager by its type-named tag or creates one when none exists.
    public static T Get()
    {
        var tag = typeof(T).Name;
        GameObject managerObject = GameObject.FindWithTag(tag);
        if (managerObject != null)
        {
            return managerObject.GetComponent<T>();
        }

        GameObject go = new(tag);
        go.tag = tag;
        return go.AddComponent<T>();
    }
}
