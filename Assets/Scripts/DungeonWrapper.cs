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
    public List<DoorWrapper> reducedDoors = new();
    public DungeonStatus dungeonStatus = DungeonStatus.Empty;

    private RoomGenerator roomGenerator;
    private DoorGenerator doorGenerator;
    private RoomReducer roomReducer;
    private CycleReducer cycleReducer;
    private ReducedDrawer reducedDrawer;

    private InputSystem_Actions inputActions;
    private InputAction IAContinue;
    private bool continueStep = false;

    // Apply global execution settings
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        CacheComponents();
        ApplyGlobalSettings();
        StartCoroutine(CheckGlobalVariablesChanged());
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
        if (IAContinue.WasPressedThisFrame() && !continueStep)
        {
            continueStep = true;
        }
    }

    private void CacheComponents()
    {
        if (TryGetComponent(out RoomGenerator roomGenerator))
        {
            this.roomGenerator = roomGenerator;
        }
        if (TryGetComponent(out DoorGenerator doorGenerator))
        {
            this.doorGenerator = doorGenerator;
        }
        if (TryGetComponent(out RoomReducer roomReducer))
        {
            this.roomReducer = roomReducer;
        }
        if (TryGetComponent(out CycleReducer cycleReducer))
        {
            this.cycleReducer = cycleReducer;
        }
        if (TryGetComponent(out ReducedDrawer reducedDrawer))
        {
            this.reducedDrawer = reducedDrawer;
        }
    }

    private void ApplyGlobalSettings()
    {
        if (roomGenerator != null)
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
        if (doorGenerator != null)
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
        if (roomReducer != null)
        {
            if (globalAutoGenerate)
            {
                roomReducer.autoGenerate = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                roomReducer.executionDelay = globalExecutionDelay;
            }
            else
            {
                roomReducer.executionDelay = 0;
            }
        }
        if (cycleReducer != null)
        {
            if (globalAutoGenerate)
            {
                cycleReducer.autoGenerate = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                cycleReducer.executionDelay = globalExecutionDelay;
            }
            else
            {
                cycleReducer.executionDelay = 0;
            }
        }
        if (reducedDrawer != null)
        {
            if (globalAutoGenerate)
            {
                reducedDrawer.autoGenerate = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                reducedDrawer.executionDelay = globalExecutionDelay;
            }
            else
            {
                reducedDrawer.executionDelay = 0;
            }
        }
    }

    private IEnumerator CheckGlobalVariablesChanged()
    {
        ExecutionDelayType currentType = executionDelayType;
        float currentDelay = globalExecutionDelay;
        while (true)
        {
            yield return new WaitUntil(() => currentType != executionDelayType || currentDelay != globalExecutionDelay);
            ApplyGlobalSettings();
        }
    }

    public IEnumerator ChangeDungeonStatus(DungeonStatus dungeonStatus)
    {
        this.dungeonStatus = dungeonStatus;
        if (executionDelayType == ExecutionDelayType.ManualBetweenSteps && dungeonStatus != DungeonStatus.RoomsDrawn)
        {
            yield return new WaitUntil(() => continueStep);
            continueStep = false;
        }
        switch (dungeonStatus)
        {
            case DungeonStatus.RoomsCompleted:
                if (doorGenerator != null)
                {
                    if (doorGenerator.autoGenerate)
                    {
                        doorGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.DoorsCompleted:
                if (roomReducer != null)
                {
                    if (roomReducer.autoGenerate)
                    {
                        roomReducer.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.RoomsReduced:
                if (cycleReducer != null)
                {
                    if (cycleReducer.autoGenerate)
                    {
                        cycleReducer.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.CyclesReduced:
                if (reducedDrawer != null)
                {
                    if (reducedDrawer.autoGenerate)
                    {
                        reducedDrawer.StartDrawing();
                    }
                }
                break;
            case DungeonStatus.RoomsDrawn:
                Debug.Log("Dungeon generation complete");
                break;
        }
    }

    public enum DungeonStatus { Empty, RoomsCompleted, DoorsCompleted, RoomsReduced, CyclesReduced, RoomsDrawn }
    public enum ExecutionDelayType { None, ManualBetweenSteps, Automatic }
}
