using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(BuildingProcess))]
// Coordinates input, unit selection, gathering commands, and building placement.
public class GameManager : SingletonManager<GameManager>
{
    public enum GameResult
    {
        Victory,
        Loss
    }

    // Unit currently selected by the player.
    public Unit ActiveUnit;
    // Scene references used for click feedback, actions, and construction.
    [SerializeField] private PointToClick m_PointToClickPrefab;
    [SerializeField] private ActionBar m_ActionBar;
    [SerializeField] private BuildingProcess m_BuildingProcess;
    [Header("Goblin Waves")]
    [FormerlySerializedAs("m_EnemyPrefab")]
    [SerializeField] private GameObject m_GoblinPrefab;
    [FormerlySerializedAs("m_EnemySpawnPosition")]
    [SerializeField] private Vector2 m_GoblinSpawnPosition = new(0f, -16f);
    [SerializeField, Min(1)] private int m_GoblinsPerWave = 3;
    [SerializeField, Min(0f)] private float m_InitialGoblinSpawnDelay = 30f;
    [FormerlySerializedAs("m_EnemySpawnInterval")]
    [SerializeField, Min(0.1f)] private float m_GoblinSpawnInterval = 2f;
    [SerializeField, Min(1f)] private float m_MatchDurationMinutes = 6f;
    // Input tracking state used to distinguish taps from drags.
    private Vector2 m_InitialTouchPosition;
    private PointToClick m_CurrentPointToClick;
    private bool m_IsTrackingClick;
    // Builder and action retained while the player chooses a build location.
    private Unit m_BuildingUnit;
    private BuildActionSO m_PendingBuildAction;
    private bool m_IsGameOver;
    private GameResult m_GameResult;
    private readonly System.Collections.Generic.HashSet<GoblinUnit> m_WaveGoblins = new();
    private int m_GoblinsSpawned;
    private int m_GoblinsDefeated;
    private float m_TimeRemaining;
    private bool m_IsTimeUp;

    // Ensures required components are available and hides the action bar at startup.
    private void Start()
    {
        Time.timeScale = 1f;
        m_TimeRemaining = m_MatchDurationMinutes * 60f;

        if (m_BuildingProcess == null)
        {
            m_BuildingProcess = GetComponent<BuildingProcess>();
            if (m_BuildingProcess == null)
            {
                m_BuildingProcess = gameObject.AddComponent<BuildingProcess>();
            }
        }

        if (m_ActionBar != null)
        {
            m_ActionBar.Show(null);
        }

        if (Camera.main != null && Camera.main.GetComponent<CameraController2D>() == null)
        {
            Camera.main.gameObject.AddComponent<CameraController2D>();
        }

        if (m_GoblinPrefab == null)
        {
            Debug.LogError("GameManager requires a Goblin prefab to start Goblin waves.", this);
        }
        else if (m_GoblinPrefab.GetComponent<GoblinUnit>() == null)
        {
            Debug.LogError("The configured Goblin prefab has no GoblinUnit component.", m_GoblinPrefab);
        }
        else
        {
            StartCoroutine(SpawnGoblinWave());
        }
    }

    // Reads mouse or touch input and forwards completed taps to click handling.
    void Update()
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_TimeRemaining = Mathf.Max(0f, m_TimeRemaining - Time.deltaTime);
        if (m_TimeRemaining <= 0f)
        {
            m_IsTimeUp = true;
            EndGame(GameResult.Victory);
            return;
        }

        bool hasTouch = Input.touchCount > 0;
        Touch touch = hasTouch ? Input.GetTouch(0) : default;
        Vector2 inputPosition = hasTouch ? touch.position : Input.mousePosition;
        bool pointerDown = hasTouch
            ? touch.phase == TouchPhase.Began
            : Input.GetMouseButtonDown(0);
        bool pointerUp = hasTouch
            ? touch.phase == TouchPhase.Ended
            : Input.GetMouseButtonUp(0);

        if (pointerDown)
        {
            m_IsTrackingClick = !IsPointerOverUI(hasTouch ? touch.fingerId : -1);
            if (m_IsTrackingClick)
            {
                m_InitialTouchPosition = inputPosition;
            }
        }

        if (pointerUp && m_IsTrackingClick)
        {
            m_IsTrackingClick = false;
            if (Vector2.Distance(m_InitialTouchPosition, inputPosition) < 10)
            {
                DetectClick(inputPosition);
            }
        }
    }

    // Waits before the first Goblin, then spawns the rest at the configured interval.
    private IEnumerator SpawnGoblinWave()
    {
        yield return new WaitForSeconds(m_InitialGoblinSpawnDelay);

        while (m_GoblinsSpawned < m_GoblinsPerWave)
        {
            if (m_IsGameOver)
            {
                yield break;
            }

            float horizontalOffset =
                (m_GoblinsSpawned % 2 == 0 ? -1f : 1f) *
                Mathf.Ceil((m_GoblinsSpawned + 1) * 0.5f) *
                1.25f;
            Vector3 spawnPosition = m_GoblinSpawnPosition + Vector2.right * horizontalOffset;
            GoblinUnit goblin = Instantiate(
                m_GoblinPrefab,
                spawnPosition,
                Quaternion.identity).GetComponent<GoblinUnit>();
            if (goblin == null)
            {
                Debug.LogError("The configured Goblin prefab has no GoblinUnit component.", m_GoblinPrefab);
                yield break;
            }

            m_WaveGoblins.Add(goblin);
            m_GoblinsSpawned++;

            if (m_GoblinsSpawned < m_GoblinsPerWave)
            {
                yield return new WaitForSeconds(m_GoblinSpawnInterval);
            }
        }
    }

    // Counts only Goblins belonging to this wave and wins when all three are defeated.
    public void NotifyGoblinDefeated(GoblinUnit goblin)
    {
        if (m_IsGameOver || goblin == null || !m_WaveGoblins.Remove(goblin))
        {
            return;
        }

        m_GoblinsDefeated++;
        if (m_GoblinsDefeated >= m_GoblinsPerWave &&
            m_GoblinsSpawned >= m_GoblinsPerWave)
        {
            EndGame(GameResult.Victory);
        }
    }

    // Stops gameplay and displays the requested final result.
    public void EndGame(GameResult result)
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_IsGameOver = true;
        m_GameResult = result;
        Time.timeScale = 0f;
        CancelBuildPlacement();
        m_ActionBar?.Show(null);
    }

    // Displays the countdown and wave HUD at the top, then the final result when the game ends.
    private void OnGUI()
    {
        int minutes = Mathf.FloorToInt(m_TimeRemaining / 60f);
        int seconds = Mathf.FloorToInt(m_TimeRemaining % 60f);
        GUI.Box(
            new Rect((Screen.width - 360f) * 0.5f, 10f, 360f, 32f),
            $"Time {minutes:00}:{seconds:00}   |   Wave 1   |   Goblins {m_GoblinsDefeated}/{m_GoblinsPerWave}");

        if (!m_IsGameOver)
        {
            return;
        }

        const float panelWidth = 320f;
        const float panelHeight = 150f;
        Rect panel = new(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);
        string resultText = m_GameResult == GameResult.Victory ? "Victory!" : "Loss";
        string details = m_GameResult == GameResult.Victory
            ? m_IsTimeUp
                ? "You survived until the timer ran out."
                : "All Goblins in Wave 1 have been defeated."
            : "Your Castle has been destroyed.";
        GUI.Box(panel, resultText);
        GUI.Label(new Rect(panel.x + 20f, panel.y + 45f, panelWidth - 40f, 35f), details);

        if (GUI.Button(new Rect(panel.x + 90f, panel.y + 95f, 140f, 35f), "Restart"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Prevents clicks on UI controls from also selecting or moving game units.
    private bool IsPointerOverUI(int pointerId)
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    // Resolves a screen click into selection, gathering, movement, or a build command.
    void DetectClick(Vector2 inputPosition)
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPoint);

        if (m_PendingBuildAction != null)
        {
            if (HasClickedOnUnit(colliders, out var clickedUnit))
            {
                CancelBuildPlacement();
                ActiveUnit = clickedUnit;
                m_ActionBar?.Show(ActiveUnit);
                return;
            }

            Unit builder = m_BuildingUnit;
            BuildActionSO action = m_PendingBuildAction;
            CancelBuildPlacement();

            if (m_BuildingProcess == null)
            {
                Debug.LogError("GameManager requires a BuildingProcess component.", this);
                return;
            }

            m_BuildingProcess.BuildAt(
                builder,
                action,
                new Vector3(worldPoint.x, worldPoint.y, 0f));
            return;
        }

        if (ActiveUnit is WorkerUnit activeWorker &&
            TryFindSheepResource(colliders, out var sheep))
        {
            activeWorker.StartGathering(sheep);
        }
        else if (HasClickedOnUnit(colliders, out var unit))
        {
            CancelActiveGathering();
            ActiveUnit = unit;
            if (m_ActionBar != null)
            {
                m_ActionBar.Show(ActiveUnit);
            }
        }
        else if (ActiveUnit is WorkerUnit worker &&
                 TryFindResourceNode(colliders, out var resourceNode))
        {
            worker.StartGathering(resourceNode);
        }
        else if (ActiveUnit != null)
        {
            if (ActiveUnit is HumanoidUnit humanoid)
            {
                humanoid.CancelAutoAttack();
            }

            CancelActiveGathering();
            ActiveUnit.MoveTo(worldPoint);
            ShowPointToClick(worldPoint);
        }
    }

    // Finds a tree resource node on the clicked collider or one of its parents.
    private bool TryFindResourceNode(Collider2D[] colliders, out TreeResourceNode resourceNode)
    {
        foreach (Collider2D collider in colliders)
        {
            resourceNode = collider.GetComponent<TreeResourceNode>();
            if (resourceNode == null)
            {
                resourceNode = collider.GetComponentInParent<TreeResourceNode>();
            }

            if (resourceNode != null)
            {
                return true;
            }
        }

        resourceNode = null;
        return false;
    }

    // Finds a sheep resource node on the clicked collider or one of its parents.
    private bool TryFindSheepResource(Collider2D[] colliders, out SheepResourceNode sheep)
    {
        foreach (Collider2D collider in colliders)
        {
            sheep = collider.GetComponent<SheepResourceNode>();
            if (sheep == null)
            {
                sheep = collider.GetComponentInParent<SheepResourceNode>();
            }

            if (sheep != null)
            {
                return true;
            }
        }

        sheep = null;
        return false;
    }

    // Cancels a worker's current gathering job when the player changes commands.
    private void CancelActiveGathering()
    {
        if (ActiveUnit is WorkerUnit worker)
        {
            worker.CancelGathering();
        }
    }

    // Stores a valid builder and action while waiting for the placement click.
    public void BeginBuildPlacement(Unit builder, BuildActionSO action)
    {
        if (builder == null || action == null)
        {
            Debug.LogError("Cannot enter build placement without a builder and build action.", this);
            return;
        }

        if (!builder.CanMove)
        {
            Debug.LogError($"Builder '{builder.name}' has no AIPawn component.", builder);
            return;
        }

        m_BuildingUnit = builder;
        m_PendingBuildAction = action;
        Debug.Log($"Click a location to build '{action.DisplayName}'.", builder);
    }

    // Clears the pending build selection after placement or cancellation.
    private void CancelBuildPlacement()
    {
        m_BuildingUnit = null;
        m_PendingBuildAction = null;
    }

    // Spawns the configured visual marker at the selected world position.
    private void ShowPointToClick(Vector2 worldPoint)
    {
        if (m_PointToClickPrefab == null)
        {
            return;
        }

        if (m_CurrentPointToClick != null)
        {
            Destroy(m_CurrentPointToClick.gameObject);
        }

        m_CurrentPointToClick = Instantiate(
            m_PointToClickPrefab,
            new Vector3(worldPoint.x, worldPoint.y, 0f),
            Quaternion.identity);
    }


    // Finds a unit on the clicked collider or its parent hierarchy.
    bool HasClickedOnUnit(Collider2D[] colliders, out Unit unit)
    {
        foreach (Collider2D collider in colliders)
        {
            Unit clickedUnit = collider.GetComponent<Unit>();
            if (clickedUnit == null)
            {
                clickedUnit = collider.GetComponentInParent<Unit>();
            }

            if (clickedUnit != null)
            {
                unit = clickedUnit;
                return true;
            }
        }

        unit = null;
        return false;
    }
}
