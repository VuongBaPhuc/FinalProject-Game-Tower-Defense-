using System.Collections;
using UnityEngine;

public class BuildingProcess : MonoBehaviour
{
    // Prevents overlapping build routines from running at the same time.
    private bool m_IsBuilding;

    // Validates the request and starts building after the selected unit reaches the site.
    public void BuildAt(Unit builder, BuildActionSO action, Vector3 position)
    {
        if (builder == null)
        {
            Debug.LogError("Cannot build because the builder no longer exists.", this);
            return;
        }

        if (action == null || action.BuildingPrefab == null)
        {
            Debug.LogError("Cannot build because the build action or building prefab is missing.", this);
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

    // Delivers carried resources, moves the builder, and completes construction at the site.
    private IEnumerator BuildRoutine(Unit builder, BuildActionSO action, Vector3 position)
    {
        m_IsBuilding = true;

        WorkerUnit worker = builder as WorkerUnit;
        if (worker != null)
        {
            if (!worker.PrepareForBuild())
            {
                m_IsBuilding = false;
                yield break;
            }

            while (builder != null && worker.HasCarriedResources)
            {
                yield return null;
            }
        }

        if (builder == null)
        {
            Debug.LogError("Building was cancelled because the builder was destroyed.", this);
            m_IsBuilding = false;
            yield break;
        }

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

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Cannot charge for the Tower because no GameManager exists in the scene.", this);
            m_IsBuilding = false;
            yield break;
        }

        if (!gameManager.TrySpendResource(action.CostType, action.CostAmount))
        {
            Debug.LogWarning(
                $"Not enough {action.CostType} to build '{action.DisplayName}'. Required: {action.CostAmount}.",
                builder);
            m_IsBuilding = false;
            yield break;
        }

        builder.PlayAnimationTrigger(action.BuildTrigger);

        GameObject building = Instantiate(action.BuildingPrefab, position, Quaternion.identity);
        Animator buildingAnimator = building.GetComponent<Animator>();
        if (buildingAnimator != null && !string.IsNullOrWhiteSpace(action.BuildingBuildTrigger))
        {
            PlayBuildAnimation(buildingAnimator, action.BuildingBuildTrigger, building);
        }

        // Save collider states so the prefab's original enabled settings can be restored.
        Collider2D[] colliders = building.GetComponentsInChildren<Collider2D>(true);
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

    // Plays the building animation when the configured Trigger exists.
    private void PlayBuildAnimation(Animator animator, string triggerName, GameObject building)
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
            $"Building Animator on '{building.name}' has no Trigger parameter named '{triggerName}'. " +
            "Leave Building Build Trigger empty if its build animation plays automatically on spawn.",
            building);
    }
}
