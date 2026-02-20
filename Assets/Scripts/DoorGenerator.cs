using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(DungeonWrapper))]
public class DoorGenerator : MonoBehaviour
{
    [Range(0, 100)][SerializeField] private int seed;

    [Header("Algorithm")]
    [Tooltip("Whether to start generating a dungeon on Start, or to wait for the Generate Dungeon button to be pressed.")]
    [SerializeField] private bool generateOnStart = true;
    [Tooltip("The time delay between generating rooms as part of the algorithm, in seconds.")]
    [Range(0, 0.1f)][SerializeField] private float executionDelay = 0.04f;

    [Space]

    [Header("Debug")]
    [SerializeField] private bool writeDebug = true;
    [SerializeField] private bool drawDoors = true;

    private System.Random numberGenerator;
    private DungeonWrapper dungeonWrapper;
    private int doorCount;
    private float time;

    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
        if (generateOnStart)
        {
            StartCoroutine(WaitForRooms());
        }
    }

    public void StartGeneration()
    {
        ClearDrawingBatchers();
        StopAllCoroutines();
        StartCoroutine(WaitForRooms());
    }

    private void ClearDrawingBatchers()
    {
        DebugDrawingBatcher.GetInstance("Doors").ClearAllBatchedCalls();
    }

    private IEnumerator WaitForRooms()
    {
        yield return new WaitUntil(()=>dungeonWrapper.dungeonStatus == DungeonWrapper.DungeonStatus.RoomsCompleted);
        StartCoroutine(GenerateDoors());
    }

    private IEnumerator GenerateDoors()
    {
        numberGenerator = new System.Random(seed);
        doorCount = 0;
        time = 0;
        for (int i = 0; i < dungeonWrapper.rooms.Count; i++)
        {
            for(int j = i+1; j < dungeonWrapper.rooms.Count; j++)
            {
                RectInt overlap = AlgorithmsUtils.Intersect(dungeonWrapper.rooms[i].room, dungeonWrapper.rooms[j].room);
                if(overlap != new RectInt())
                {
                    DrawDoor(overlap, Color.blue, "Doors");
                    doorCount++;
                    if (executionDelay > 0)
                    {
                        yield return new WaitForSeconds(executionDelay);
                    }
                    //if(overlap.width > overlap.height)
                    //{
                    //
                    //}
                    //else
                    //{
                    //
                    //}
                }
                
            }
        }
        WriteDebug("Door generation complete. " + doorCount + " doors generated successfullly, in " + (Time.time - time) + " Seconds.");
        dungeonWrapper.dungeonStatus = DungeonWrapper.DungeonStatus.DoorsCompleted;
    }

    private void DrawDoor(RectInt door, Color color, string debugDrawer = "default")
    {
        //Debug Drawing Batcher must use a value instead of a reference, so it cannot use currentBTEntry.room data as that is a reference type.
        RectInt currentDoor = door;
        DebugDrawingBatcher.GetInstance(debugDrawer).BatchCall(() => AlgorithmsUtils.DebugRectInt(currentDoor, color));
    }

    private void WriteDebug(object message)
    {
        if (writeDebug)
        {
            Debug.Log(message);
        }
    }
}
