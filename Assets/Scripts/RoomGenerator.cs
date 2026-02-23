using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Computational complexity: O(2n-1), with n being the amount of rooms generated. Theoretical limit of rooms generated is floor(DungeonWidth/minRoomWidth) * floor(DungeonHeight/minRoomHeight)
/// </summary>
[RequireComponent(typeof(DungeonWrapper))]
public class RoomGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [Tooltip("Random number generator seed")]
    [Range(0, 100)][SerializeField] private int seed = 40;
    [Tooltip("Size of the whole dungeon in meters. X is width, Y is height.")]
    [SerializeField] private Vector2 dungeonSize = new Vector2(150, 250);

    [Space]

    [Header("Room Settings")]
    [Tooltip("Minimum room size in meters. X is width, Y is height.")]
    [SerializeField] private Vector2 roomMinSize = new Vector2(20, 25);
    [Tooltip("Decimal chance for the room generating algorithm to split rooms horizontally or vertically. 0 represents an always-horizontal generation. 1 represents an always-vertical generation.")]
    [Range(0, 1)] [SerializeField] private float splitDirectionBias = 0.57f;
    [Tooltip("Maximum room size when generating large rooms, in meters. X is width, Y is height.")]
    [SerializeField] private Vector2 roomMaxSize = new Vector2(60, 60);
    [Tooltip("Decimal chance for room generating ending early if a room is smaller than the Room Max Size. 0 represents 0% chance, and 1 represents 100% chance.")]
    [Range(0, 1)] [SerializeField] private float largeRoomChance = 0.57f;

    [Space]

    [Header("Algorithm")]
    [Tooltip("Whether to start generating a dungeon on Start, or to wait for the Generate Dungeon button to be pressed.")]
    [SerializeField] private bool generateOnStart = true;
    [Tooltip("The time delay between generating rooms as part of the algorithm, in seconds.")]
    [Range(0, 0.1f)] [SerializeField] private float executionDelay = 0.02f;

    [Space]

    [Header("Debug")]
    [SerializeField] private bool writeDebug = true;
    [SerializeField] private bool drawDungeonGeneration = true;
    [SerializeField] private bool drawRooms = true;

    private System.Random numberGenerator;
    private DungeonWrapper dungeonWrapper;
    float time; // Debug value. Shows the time needed to complete the generation.
    int cycles; // Debug value. Shows the amount of cycles needed to complete the generation.
    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
        if (generateOnStart)
        {
            StartCoroutine(GenerateDungeon());
        }
    }
    public void StartGeneration()
    {
        WriteDebug("Resetting rooms...");
        ClearDrawingBatchers();
        StopAllCoroutines();
        StartCoroutine(GenerateDungeon());
    }

    private void ClearDrawingBatchers()
    {
        DebugDrawingBatcher.GetInstance("Rooms").ClearAllBatchedCalls();
        DebugDrawingBatcher.GetInstance("Leafs").ClearAllBatchedCalls();
        DebugDrawingBatcher.GetInstance("Doors").ClearAllBatchedCalls();
        DebugDrawingBatcher.GetInstance("Connections").ClearAllBatchedCalls();
    }

    private IEnumerator GenerateDungeon()
    {
        //Reset random number generator.
        numberGenerator = new System.Random(seed);

        
        dungeonWrapper.dungeonStatus = DungeonWrapper.DungeonStatus.Empty;

        //Start the dungeon generation loop, starting at the starting point of the dungeon.
        dungeonWrapper.rooms = new();
        time = Time.time;
        cycles = 0;
        WriteDebug("Starting room generation...");

        //Generate the starting point of the dungeon, then recusively call the GenerateRoom function within itself to split the room into smaller rooms
        yield return StartCoroutine(CheckRoomComplete(new RoomWrapper(new RectInt(0, 0, (int)dungeonSize.x, (int)dungeonSize.y))));

        WriteDebug("Room generation complete. " + dungeonWrapper.rooms.Count + " rooms generated successfullly, in " + cycles + " cycles, spanning " + (Time.time - time) + " seconds.");
        dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.RoomsCompleted);
    }

    private IEnumerator CheckRoomComplete(RoomWrapper currentRoom)
    {
        cycles++;
        if (currentRoom.room.width < roomMinSize.x * 2 && currentRoom.room.height < roomMinSize.y * 2)
        {
            CompleteRoom(currentRoom);
            yield break;
        }
        //Random chance to have larger rooms within the generated dungeon.
        if (currentRoom.room.width < roomMaxSize.x && currentRoom.room.height < roomMaxSize.y)
        {
            if (numberGenerator.NextDouble() < largeRoomChance)
            {
                CompleteRoom(currentRoom);
                yield break;
            }
        }
        //We have found a room that we want to split

        //Delay execution and draw the parent room
        if (executionDelay > 0)
        {
            yield return new WaitForSeconds(executionDelay);
        }
        if (drawDungeonGeneration)
        {
            DrawRoom(currentRoom, Color.yellow, "Rooms");
        }

        yield return StartCoroutine(SplitRoom(currentRoom));
    }

    private void DrawRoom(RoomWrapper currentRoom, Color color, string debugDrawer = "default")
    {
        //Debug Drawing Batcher must use a value instead of a reference, so it cannot use currentBTEntry.room data as that is a reference type.
        RectInt room = currentRoom.room;
        DebugDrawingBatcher.GetInstance(debugDrawer).BatchCall(() => AlgorithmsUtils.DebugRectInt(room, color));
    }

    private void CompleteRoom(RoomWrapper currentRoom)
    {
        dungeonWrapper.rooms.Add(currentRoom);
        //Make the room 1 unit larger in every direction to allow the rooms to overlap for future algorithms.
        currentRoom.room = new RectInt(currentRoom.room.position.x - 1, currentRoom.room.position.y - 1, currentRoom.room.width + 2, currentRoom.room.height + 2);
        if (drawRooms)
        {
            DrawRoom(currentRoom, Color.red, "Leafs");
        }
    }

    private IEnumerator SplitRoom(RoomWrapper currentRoom)
    {
        //Randomly select whether to attempt to horizontally or vertically split first, then attempt splitting that way if the room is large enough.
        if (numberGenerator.NextDouble() < splitDirectionBias)
        {
            if (currentRoom.room.width >= roomMinSize.x * 2)
            {
                yield return StartCoroutine(SplitHorizontally(currentRoom));
            }
            else
            {
                yield return StartCoroutine(SplitVertically(currentRoom));
            }
        }
        else
        {
            if (currentRoom.room.height >= roomMinSize.y * 2)
            {
                yield return StartCoroutine(SplitVertically(currentRoom));
            }
            else
            {
                yield return StartCoroutine(SplitHorizontally(currentRoom));
            }
        }
    }

    private IEnumerator SplitHorizontally(RoomWrapper currentRoom)
    {
        int splitPointX = numberGenerator.Next((int)roomMinSize.x, (int)(currentRoom.room.width - roomMinSize.x));
        yield return StartCoroutine(CheckRoomComplete(new RoomWrapper(new RectInt(currentRoom.room.position.x, currentRoom.room.position.y, splitPointX, currentRoom.room.height))));
        yield return StartCoroutine(CheckRoomComplete(new RoomWrapper(new RectInt(currentRoom.room.position.x + splitPointX, currentRoom.room.position.y, currentRoom.room.width - splitPointX, currentRoom.room.height))));
    }

    private IEnumerator SplitVertically(RoomWrapper currentRoom)
    {
        int splitPointY = numberGenerator.Next((int)roomMinSize.y, (int)(currentRoom.room.height - roomMinSize.y));
        yield return StartCoroutine(CheckRoomComplete(new RoomWrapper(new RectInt(currentRoom.room.position.x, currentRoom.room.position.y, currentRoom.room.width, splitPointY))));
        yield return StartCoroutine(CheckRoomComplete(new RoomWrapper(new RectInt(currentRoom.room.position.x, currentRoom.room.position.y + splitPointY, currentRoom.room.width, currentRoom.room.height - splitPointY))));
    }

    private void WriteDebug(object message)
    {
        if (writeDebug)
        {
            Debug.Log(message);
        }
    }
}
