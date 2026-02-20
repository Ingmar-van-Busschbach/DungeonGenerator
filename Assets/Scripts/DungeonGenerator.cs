using NaughtyAttributes;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DungeonWrapper))]
public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [Tooltip("Size of the whole dungeon in meters. X is width, Y is height.")]
    [SerializeField] private Vector2 dungeonSize = new Vector2(50, 100);
    [Tooltip("Random number generator seed")]
    [Range(0, 100)][SerializeField] private int seed;

    [Space]

    [Header("Room Settings")]
    [Tooltip("Minimum room size in meters. X is width, Y is height.")]
    [SerializeField] private Vector2 roomMinSize = new Vector2(5, 10);
    [Tooltip("Decimal chance for the room generating algorithm to split rooms horizontally or vertically. 0 represents an always-horizontal generation. 1 represents an always-vertical generation.")]
    [Range(0, 1)] [SerializeField] private float splitDirectionBias = 0.5f;
    [Tooltip("Maximum room size when generating large rooms, in meters. X is width, Y is height.")]
    [SerializeField] private Vector2 roomMaxSize = new Vector2(25, 25);
    [Tooltip("Decimal chance for room generating ending early if a room is smaller than the Room Max Size. 0 represents 0% chance, and 1 represents 100% chance.")]
    [Range(0, 1)] [SerializeField] private float largeRoomChance = 0.1f;

    [Space]

    [Header("Algorithm")]
    [Tooltip("Whether to start generating a dungeon on Start, or to wait for the Generate Dungeon button to be pressed.")]
    [SerializeField] private bool generateOnStart = true;
    [Tooltip("The time delay between generating rooms as part of the algorithm, in seconds.")]
    [Range(0, 0.1f)] [SerializeField] private float executionDelay = 0.04f;

    [Space]

    [Header("Debug")]
    [SerializeField] private bool drawDungeonGeneration = true;
    [SerializeField] private bool drawRooms = true;

    private System.Random numberGenerator;
    private DungeonWrapper dungeonWrapper;
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
        DebugDrawingBatcher.GetInstance().ClearAllBatchedCalls();
        StopAllCoroutines();
        StartCoroutine(GenerateDungeon());
    }

    private IEnumerator GenerateDungeon()
    {
        //Reset random number generator.
        numberGenerator = new System.Random(seed);

        //Generate the starting point of the dungeon.
        dungeonWrapper.origin = new BTEntry(null, new RectInt(0, 0, (int)dungeonSize.x, (int)dungeonSize.y));
        dungeonWrapper.dungeonStatus = DungeonWrapper.DungeonStatus.Empty;

        //Start the dungeon generation loop, starting at the starting point of the dungeon.
        BTEntry currentBTEntry;
        currentBTEntry = dungeonWrapper.origin;
        while (dungeonWrapper.origin.complete != BTEntry.BTEntryStatus.Complete)
        {
            //Check if we completed the current entry since the last cycle. If we did, loop back to the parent.
            if (currentBTEntry.complete != BTEntry.BTEntryStatus.Complete)
            {
                currentBTEntry.CheckComplete();
            }
            if (currentBTEntry.complete == BTEntry.BTEntryStatus.Complete)
            {
                currentBTEntry = currentBTEntry.parent;
                continue;
            }

            //Delay the algorithm and draw the room.
            yield return new WaitForSeconds(executionDelay);
            if (drawDungeonGeneration)
            {
                DrawRoom(currentBTEntry, Color.yellow);
            }

            //Check if the current entry can be divided into smaller rooms. If not, mark as completed.
            if (currentBTEntry.room.width < roomMinSize.x * 2 && currentBTEntry.room.height < roomMinSize.y * 2)
            {
                CompleteRoom(currentBTEntry);
                continue;
            }
            //Select left room if generation was already done on the right branch but not on the left.
            if (currentBTEntry.right != null)
            {
                if (currentBTEntry.right.complete == BTEntry.BTEntryStatus.Complete && currentBTEntry.left.complete != BTEntry.BTEntryStatus.Complete)
                {
                    currentBTEntry = currentBTEntry.left;
                    continue;
                }
            }

            //We have found a Binary Tree Entry to split into two rooms or complete as a large room.

            //Random chance to have larger rooms within the generated dungeon.
            if (currentBTEntry.room.width < roomMaxSize.x && currentBTEntry.room.height < roomMaxSize.y)
            {
                if (numberGenerator.NextDouble() < largeRoomChance)
                {
                    CompleteRoom(currentBTEntry);
                    continue;
                }
            }

            //Split current room into two.
            currentBTEntry = SplitRoom(currentBTEntry);
        }

        //Mark the current generation step as completed, so that future algorithms can wait with executing until this step is completed.
        dungeonWrapper.dungeonStatus = DungeonWrapper.DungeonStatus.RoomsCompleted;
    }

    private void DrawRoom(BTEntry currentBTEntry, Color color)
    {
        //Debug Drawing Batcher must use a value instead of a reference, so it cannot use currentBTEntry.room data as that is a reference type.
        RectInt room = currentBTEntry.room;
        DebugDrawingBatcher.GetInstance().BatchCall(() => AlgorithmsUtils.DebugRectInt(room, color));
    }

    private void CompleteRoom(BTEntry currentBTEntry)
    {
        currentBTEntry.complete = BTEntry.BTEntryStatus.Complete;
        //Leafs are used by future algorithms
        currentBTEntry.leaf = true;
        //Make the room 1 unit larger in every direction to allow the rooms to overlap for future algorithms.
        currentBTEntry.room = new RectInt(currentBTEntry.room.position.x - 1, currentBTEntry.room.position.y - 1, currentBTEntry.room.width + 2, currentBTEntry.room.height + 2);
        if (drawRooms)
        {
            DrawRoom(currentBTEntry, Color.red);
        }
    }

    private BTEntry SplitRoom(BTEntry currentBTEntry)
    {
        //Randomly select whether to attempt to horizontally or vertically split first, then attempt splitting that way if the room is large enough.
        if (numberGenerator.NextDouble() < splitDirectionBias)
        {
            if (currentBTEntry.room.width >= roomMinSize.x * 2)
            {
                return SplitHorizontally(currentBTEntry);
            }
            else
            {
                return SplitVertically(currentBTEntry);
            }
        }
        else
        {
            if (currentBTEntry.room.height >= roomMinSize.y * 2)
            {
                return SplitVertically(currentBTEntry);
            }
            else
            {
                return SplitHorizontally(currentBTEntry);
            }
        }
    }
    
    private BTEntry SplitHorizontally(BTEntry currentBTEntry)
    {
        int splitPointX = numberGenerator.Next((int)roomMinSize.x, (int)(currentBTEntry.room.width - roomMinSize.x));
        currentBTEntry.right = new BTEntry(currentBTEntry, new RectInt(currentBTEntry.room.position.x, currentBTEntry.room.position.y, splitPointX, currentBTEntry.room.height));
        currentBTEntry.left = new BTEntry(currentBTEntry, new RectInt(currentBTEntry.room.position.x + splitPointX, currentBTEntry.room.position.y, currentBTEntry.room.width - splitPointX, currentBTEntry.room.height));
        return currentBTEntry.right;
    }

    private BTEntry SplitVertically(BTEntry currentBTEntry)
    {
        int splitPointY = numberGenerator.Next((int)roomMinSize.y, (int)(currentBTEntry.room.height - roomMinSize.y));
        currentBTEntry.right = new BTEntry(currentBTEntry, new RectInt(currentBTEntry.room.position.x, currentBTEntry.room.position.y, currentBTEntry.room.width, splitPointY));
        currentBTEntry.left = new BTEntry(currentBTEntry, new RectInt(currentBTEntry.room.position.x, currentBTEntry.room.position.y + splitPointY, currentBTEntry.room.width, currentBTEntry.room.height - splitPointY));
        return currentBTEntry.right;
    }
}
