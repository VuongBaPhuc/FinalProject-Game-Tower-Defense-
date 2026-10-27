using System.Collections;
using UnityEngine;

public class BuildingProcess : MonoBehaviour
{
    private bool m_IsBuilding;

    public void BuildAt(Unit builder, BuildActionSO action, Vector3 position)
    {
        if (builder == null)
        {
            Debug.LogError("Cannot build because the builder no longer exists.", this);
            return;
        }

        if (action == null || action.TowerPrefab == null)
        {
            Debug.LogError("Cannot build because the build action or Tower prefab is missing.", this);
            return;
        }

        if (!builder.CanMove)
        {
            Debug.LogError($"Builder '{builder.name}' has no AIPawn component.", builder);
            return;
        }

        if (m_IsBuilding)
        {
            Debug.LogWarning("A building process is already in progress.", this);
            return;
        }

        StartCoroutine(BuildRoutine(builder, action, position));
    }

    private IEnumerator BuildRoutine(Unit builder, BuildActionSO action, Vector3 position)
    {
        m_IsBuilding = true;
        builder.MoveTo(position);

        while (builder != null && builder.HasPendingMove)
        {
            yield return null;
        }

        if (builder == null)
        {
            Debug.LogError("Building was cancelled because the builder was destroyed.", this);
            m_IsBuilding = false;
            yield break;
        }

        builder.PlayAnimationTrigger(action.BuildTrigger);

        GameObject tower = Instantiate(action.TowerPrefab, position, Quaternion.identity);
        Animator towerAnimator = tower.GetComponent<Animator>();
        if (towerAnimator == null)
        {
            Debug.LogError($"Tower prefab '{tower.name}' has no Animator component.", tower);
        }
        else if (!string.IsNullOrWhiteSpace(action.TowerBuildTrigger))
        {
            PlayTowerBuildAnimation(towerAnimator, action.TowerBuildTrigger, tower);
        }

        Collider2D[] colliders = tower.GetComponentsInChildren<Collider2D>(true);
        bool[] colliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            colliderStates[i] = colliders[i].enabled;
            colliders[i].enabled = false;
        }

        float duration = action.BuildDuration;
        yield return new WaitForSeconds(duration);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = colliderStates[i];
            }
        }

        m_IsBuilding = false;
    }

    private void PlayTowerBuildAnimation(Animator animator, string triggerName, GameObject tower)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == triggerName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }

        Debug.LogError(
            $"Tower Animator on '{tower.name}' has no Trigger parameter named '{triggerName}'. " +
            "Leave Tower Build Trigger empty if its build animation plays automatically on spawn.",
            tower);
    }
}
