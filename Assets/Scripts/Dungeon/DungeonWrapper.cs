using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.ComponentModel;

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
    public bool[,] tileMap;
    public DungeonStatus dungeonStatus = DungeonStatus.Empty;

    private RoomGenerator roomGenerator;
    private DoorGenerator doorGenerator;
    private RoomReducer roomReducer;
    private CycleReducer cycleReducer;
    private ReducedDrawer reducedDrawer;
    private TileMapGenerator tileMapGenerator;
    private MarchingSquareSpawner marchingSquareSpawner;
    private FloodFillSpawner floodFillSpawner;
    private PlayerSpawner playerSpawner;
    private NavMeshGenerator navMeshGenerator;

    private HashSet<ProceduralGenerator> proceduralGenerators = new();

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
            proceduralGenerators.Add(roomGenerator);
        }
        if (TryGetComponent(out DoorGenerator doorGenerator))
        {
            this.doorGenerator = doorGenerator;
            proceduralGenerators.Add(doorGenerator);
        }
        if (TryGetComponent(out RoomReducer roomReducer))
        {
            this.roomReducer = roomReducer;
            proceduralGenerators.Add(roomReducer);
        }
        if (TryGetComponent(out CycleReducer cycleReducer))
        {
            this.cycleReducer = cycleReducer;
            proceduralGenerators.Add(cycleReducer);
        }
        if (TryGetComponent(out ReducedDrawer reducedDrawer))
        {
            this.reducedDrawer = reducedDrawer;
            proceduralGenerators.Add(reducedDrawer);
        }
        if(TryGetComponent(out TileMapGenerator tileMapGenerator))
        {
            this.tileMapGenerator = tileMapGenerator;
            proceduralGenerators.Add(tileMapGenerator);
        }
        if(TryGetComponent(out MarchingSquareSpawner marchingSquareSpawner))
        {
            this.marchingSquareSpawner = marchingSquareSpawner;
            proceduralGenerators.Add(marchingSquareSpawner);
        }
        if (TryGetComponent(out FloodFillSpawner floodFillSpawner))
        {
            this.floodFillSpawner = floodFillSpawner;
            proceduralGenerators.Add(floodFillSpawner);
        }
        if(TryGetComponent(out PlayerSpawner playerSpawner))
        {
            this.playerSpawner = playerSpawner;
            proceduralGenerators.Add(playerSpawner);
        }
        if (TryGetComponent(out NavMeshGenerator navMeshGenerator))
        {
            this.navMeshGenerator = navMeshGenerator;
            proceduralGenerators.Add(navMeshGenerator);
        }
    }

    private void ApplyGlobalSettings()
    {
        foreach(ProceduralGenerator generator in proceduralGenerators)
        {
            if (globalAutoGenerate)
            {
                generator.autoGenerate = true;
            }
            if (executionDelayType != ExecutionDelayType.None)
            {
                generator.executionDelay = globalExecutionDelay;
            }
            else
            {
                generator.executionDelay = 0;
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
        if (executionDelayType == ExecutionDelayType.ManualBetweenSteps && dungeonStatus != DungeonStatus.DungeonGenerationComplete)
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
                if (tileMapGenerator != null)
                {
                    if (tileMapGenerator.autoGenerate)
                    {
                        tileMapGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.TileMapGenerated:
                if (marchingSquareSpawner != null)
                {
                    if (marchingSquareSpawner.autoGenerate)
                    {
                        marchingSquareSpawner.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.WallsSpawned:
                if(floodFillSpawner  != null)
                {
                    if (floodFillSpawner.autoGenerate)
                    {
                        floodFillSpawner.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.FloorSpawned:
                if (navMeshGenerator != null)
                {
                    if (navMeshGenerator.autoGenerate)
                    {
                        navMeshGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.NavMeshGenerated:
                if (playerSpawner != null)
                {
                    if (playerSpawner.autoGenerate)
                    {
                        playerSpawner.StartSpawning();
                    }
                }
                break;
            case DungeonStatus.PlayerSpawned:
                StartCoroutine(ChangeDungeonStatus(DungeonStatus.DungeonGenerationComplete));
                break;
            case DungeonStatus.DungeonGenerationComplete:
                Debug.Log("Dungeon generation complete");
                break;
        }
    }

    public enum DungeonStatus { Empty, RoomsCompleted, DoorsCompleted, RoomsReduced, CyclesReduced, RoomsDrawn, TileMapGenerated, WallsSpawned, FloorSpawned, NavMeshGenerated, PlayerSpawned, DungeonGenerationComplete }
    public enum ExecutionDelayType { None, ManualBetweenSteps, Automatic }
}
