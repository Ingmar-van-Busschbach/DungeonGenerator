using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Computational complexity: O(0.5n^2 - 0.5n), with n being the starting amount of rooms.
/// </summary>

[RequireComponent(typeof(DungeonWrapper))]
public class DoorGenerator : MonoBehaviour
{
    [Header("Door Settings")]
    [Range(0, 100)][SerializeField] private int seed = 90;
    [SerializeField] private int doorMinSize = 5;
    [SerializeField] private int doorMaxSize = 10;
    [SerializeField] private int minDistanceFromWalls = 2;

    [Header("Algorithm")]
    [Tooltip("Whether to start generating a dungeon on Start, or to wait for the Generate Dungeon button to be pressed.")]
    public bool autoGenerate = true;
    [Tooltip("The time delay between generating rooms as part of the algorithm, in seconds.")]
    [Range(0, 0.1f)][SerializeField] private float executionDelay = 0.02f;

    [Space]

    [Header("Debug")]
    [SerializeField] private bool writeDebug = true;
    [SerializeField] private bool drawDoors = true;

    private System.Random numberGenerator;
    private DungeonWrapper dungeonWrapper;
    private float time; // Debug value. Shows the time needed to complete the generation.
    private int cycles; // Debug value. Shows the amount of cycles needed to complete the generation.

    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }

    public void StartGeneration()
    {
        if(dungeonWrapper.dungeonStatus == DungeonWrapper.DungeonStatus.RoomsCompleted)
        {
            ClearDrawingBatchers();
            StopAllCoroutines();
            StartCoroutine(GenerateDoors());
        }
    }

    private void ClearDrawingBatchers()
    {
        DebugDrawingBatcher.GetInstance("Doors").ClearAllBatchedCalls();
    }

    private IEnumerator GenerateDoors()
    {
        numberGenerator = new System.Random(seed);
        dungeonWrapper.doors = new();
        time = Time.time;
        cycles = 0;
        for (int i = 0; i < dungeonWrapper.rooms.Count; i++)
        {
            for(int j = i+1; j < dungeonWrapper.rooms.Count; j++)
            {
                cycles++;

                //Returns new RectInt(); if there is no overlap
                RectInt currentDoor = AlgorithmsUtils.Intersect(dungeonWrapper.rooms[i].room, dungeonWrapper.rooms[j].room);
                if(currentDoor != new RectInt())
                {
                    //Automatically select whether the door is drawn horizontally or vertically depending on whether it's wider or taller.
                    if(currentDoor.width > currentDoor.height)
                    {
                        currentDoor = HorizontalDoor(currentDoor);
                    }
                    else
                    {
                        currentDoor = VerticalDoor(currentDoor);
                    }
                    //If a valid door could not be created due to not enough space in the room, discard this cycle.
                    if(currentDoor == new RectInt())
                    {
                        continue;
                    }
                    dungeonWrapper.doors.Add(currentDoor);
                    dungeonWrapper.rooms[i].doors.Add(currentDoor);
                    dungeonWrapper.rooms[j].doors.Add(currentDoor);
                    dungeonWrapper.rooms[i].connectingRooms.Add(dungeonWrapper.rooms[j]);
                    dungeonWrapper.rooms[j].connectingRooms.Add(dungeonWrapper.rooms[i]);

                    if (executionDelay > 0)
                    {
                        yield return new WaitForSeconds(executionDelay);
                    }
                    if (drawDoors)
                    {
                        DrawDoor(currentDoor, Color.blue, "Doors");
                    }

                }
                
            }
        }
        WriteDebug("Door generation complete. " + dungeonWrapper.doors.Count + " doors generated successfullly, in " + cycles + " cycles, spanning " + (Time.time - time) + " seconds.");
        dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.DoorsCompleted);
    }

    private RectInt HorizontalDoor(RectInt currentDoor)
    {
        //Recreate the overlap with removal of the corners that would overlap with other overlaps,
        currentDoor = new RectInt(currentDoor.position.x + currentDoor.height + minDistanceFromWalls, currentDoor.position.y, currentDoor.width - 2 * currentDoor.height - 2 * minDistanceFromWalls, currentDoor.height);
        if (currentDoor.width < doorMinSize)
        {
            return new RectInt();
        }
        int randomWidth = numberGenerator.Next(doorMinSize, Mathf.Min(doorMaxSize, currentDoor.width));
        int randomPosition = currentDoor.position.x;
        if (randomWidth < currentDoor.width)
        {
            randomPosition = numberGenerator.Next(currentDoor.position.x, currentDoor.position.x + currentDoor.width - randomWidth);
        }
        currentDoor = new RectInt(randomPosition, currentDoor.position.y, randomWidth, currentDoor.height);
        return currentDoor;
    }

    private RectInt VerticalDoor(RectInt currentDoor)
    {
        currentDoor = new RectInt(currentDoor.position.x, currentDoor.position.y + currentDoor.width + minDistanceFromWalls, currentDoor.width, currentDoor.height - 2 * currentDoor.width - 2 * minDistanceFromWalls);
        if (currentDoor.height < doorMinSize)
        {
            return new RectInt();
        }
        int randomHeight = numberGenerator.Next(doorMinSize, Mathf.Min(doorMaxSize, currentDoor.height));
        int randomPosition = currentDoor.position.y;
        if(randomHeight < currentDoor.height)
        {
            randomPosition = numberGenerator.Next(currentDoor.position.y, currentDoor.position.y + currentDoor.height - randomHeight);
        }
        currentDoor = new RectInt(currentDoor.position.x, randomPosition, currentDoor.width, randomHeight);
        return currentDoor;
    }

    private void DrawDoor(RectInt currentDoor, Color color, string debugDrawer = "default")
    {
        //Debug Drawing Batcher must use a value instead of a reference, so it cannot use currentDoor data as that is a reference type.
        RectInt door = currentDoor;
        DebugDrawingBatcher.GetInstance(debugDrawer).BatchCall(() => AlgorithmsUtils.DebugRectInt(door, color));
    }

    private void WriteDebug(object message)
    {
        if (writeDebug)
        {
            Debug.Log(message);
        }
    }
}
