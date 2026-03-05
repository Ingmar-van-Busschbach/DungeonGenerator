using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonWrapper : MonoBehaviour
{
    
    [Header("Algorithm")]
    [Tooltip("Whether to start generating connections on Start, or to wait for the Generate Connections button to be pressed. This is a global variable overriding the individual algorithms.")]
    public bool globalAutoGenerate = true;
    public ExecutionDelayType executionDelayType = ExecutionDelayType.ManualBetweenSteps;
    [Tooltip("The time delay between generating as part of the algorithm, in seconds. This is a global variable overriding the individual algorithms.")]
    [Range(0, 0.1f)][SerializeField] private float globalExecutionDelay = 0.01f;

    
    public List<RoomWrapper> rooms = new();
    public List<RoomWrapper> reducedRooms = new();
    public List<DoorWrapper> doors = new();
    public DungeonStatus dungeonStatus = DungeonStatus.Empty;

    private InputSystem_Actions inputActions;
    private InputAction IAContinue;
    private bool continueStep = false;

    // Apply global execution settings
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        ApplyGlobalSettings();
        StartCoroutine(CheckExecutionTypeChanged());
        StartCoroutine(CheckExecutionDelayChanged());
    }

    //Declare inputs
    private void OnEnable()
    {
        IAContinue = inputActions.Player.Jump;
        IAContinue.Enable();
    }

    private void OnDisable()
    {
        IAContinue.Disable();
    }

    private void Update()
    {
        if (IAContinue.WasPressedThisFrame())
        {
            continueStep = true;
        }
    }

    private void ApplyGlobalSettings()
    {
        if (TryGetComponent(out RoomGenerator roomGenerator))
        {
            if (globalAutoGenerate)
            {
                roomGenerator.generateOnStart = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                roomGenerator.executionDelay = globalExecutionDelay;
            }
            else
            {
                roomGenerator.executionDelay = 0;
            }
        }
        if (TryGetComponent(out DoorGenerator doorGenerator))
        {
            if (globalAutoGenerate)
            {
                doorGenerator.autoGenerate = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                doorGenerator.executionDelay = globalExecutionDelay;
            }
            else
            {
                doorGenerator.executionDelay = 0;
            }
        }
        if (TryGetComponent(out ConnectionGenerator connectionGenerator))
        {
            if (globalAutoGenerate)
            {
                connectionGenerator.autoGenerate = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                connectionGenerator.executionDelay = globalExecutionDelay;
            }
            else
            {
                connectionGenerator.executionDelay = 0;
            }
        }
    }

    private IEnumerator CheckExecutionTypeChanged()
    {
        ExecutionDelayType currentType = executionDelayType;
        while (true)
        {
            yield return new WaitUntil(() => currentType != executionDelayType);
            ApplyGlobalSettings();
        }
    }

    private IEnumerator CheckExecutionDelayChanged()
    {
        float currentDelay = globalExecutionDelay;
        while (true)
        {
            yield return new WaitUntil(() => currentDelay != globalExecutionDelay);
            ApplyGlobalSettings();
        }
    }

    public IEnumerator ChangeDungeonStatus(DungeonStatus dungeonStatus)
    {
        this.dungeonStatus = dungeonStatus;
        if (executionDelayType == ExecutionDelayType.ManualBetweenSteps && dungeonStatus != DungeonStatus.ConnectionsCompleted)
        {
            yield return new WaitUntil(() => continueStep);
            continueStep = false;
        }
        switch (dungeonStatus)
        {
            case DungeonStatus.RoomsCompleted:
                if (TryGetComponent(out DoorGenerator doorGenerator))
                {
                    if (doorGenerator.autoGenerate)
                    {
                        doorGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.DoorsCompleted:
                if (TryGetComponent(out ConnectionGenerator connectionGenerator))
                {
                    if (connectionGenerator.autoGenerate)
                    {
                        connectionGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.ConnectionsCompleted:
                Debug.Log("Dungeon generation complete");
                break;
        }
    }

    public enum DungeonStatus { Empty, RoomsCompleted, DoorsCompleted, ConnectionsCompleted }
    public enum ExecutionDelayType { None, ManualBetweenSteps, Automatic }
}
