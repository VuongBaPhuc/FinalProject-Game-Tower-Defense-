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

    // Moves the builder, spawns the tower, disables its colliders while building, then restores them.
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

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Cannot charge for the Tower because no GameManager exists in the scene.", this);
            m_IsBuilding = false;
            yield break;
        }

        if (!gameManager.TrySpendWood(action.WoodCost))
        {
            Debug.LogWarning(
                $"Not enough Wood to build '{action.DisplayName}'. Required: {action.WoodCost}.",
                builder);
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

        // Save collider states so the prefab's original enabled settings can be restored.
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

    // Plays the tower's build animation when the configured Trigger exists.
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
