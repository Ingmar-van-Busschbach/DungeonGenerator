using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(DungeonWrapper))]
public class CycleReducer : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("The ratio of smallest rooms that will be removed from the dungeon, if able.")]
    [Range(0, 1)] [SerializeField] private float removeRatio = 0.1f;
    [SerializeField] private bool removeLoops = true;

    [Space]

    [Header("Algorithm")]
    [Tooltip("Whether to start generating connections on Start, or to wait for the Generate Connections button to be pressed.")]
    public bool autoGenerate = true;
    [Tooltip("The time delay between generating rooms as part of the algorithm, in seconds.")]
    [Range(0, 0.1f)] public float executionDelay = 0.02f;

    [Space]

    [Header("Debug")]
    [SerializeField] private bool writeDebug = true;
    [SerializeField] private bool drawConnections = true;

    private DungeonWrapper dungeonWrapper;


    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }

    public void StartGeneration()
    {
        ClearDrawingBatchers();
        StopAllCoroutines();
        StartCoroutine(GenerateConnections());
    }

    private void ClearDrawingBatchers()
    {
        DebugDrawingBatcher.GetInstance("Connections").ClearAllBatchedCalls();
    }

    private IEnumerator GenerateConnections()
    {
        //Start execution timer
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        float time = Time.time;
        WriteDebug("Starting connection generation...");
        foreach (RoomWrapper room in dungeonWrapper.reducedRooms)
        {
            foreach (DoorWrapper door in room.doors)
            {
                if (!dungeonWrapper.reducedDoors.Contains(door))
                {
                    dungeonWrapper.reducedDoors.Add(door);
                }
            }
        }
        int removeCount = 0;
        ReduceDoors(dungeonWrapper.reducedDoors, ref removeCount);
        foreach (RoomWrapper room in dungeonWrapper.reducedRooms)
        {
            if (executionDelay > 0)
            {
                yield return new WaitForSeconds(executionDelay);
            }
            foreach (DoorWrapper door in room.doors)
            {
                if (drawConnections)
                {
                    DrawConnection(room.room.center, door.door.center);
                }
            }
        }
        WriteDebug("Door reduction complete. " + removeCount + " doors were removed successfullly, spanning " + (executionDelay > 0 ? ((Time.time - time)) + " seconds." : (stopwatch.ElapsedMilliseconds + "ms.")));
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.CyclesReduced));
    }

    private void ReduceDoors(List<DoorWrapper> doors, ref int removeCount)
    {     
        for(int i = 0; i < doors.Count; i++)
        {
            if (CanRemoveDoor(doors[i], doors))
            {
                removeCount++;
                RemoveDoor(doors, doors[i]);
            }
        }
    }

    private bool CanRemoveDoor(DoorWrapper door, List<DoorWrapper> doors)
    {
        HashSet<DoorWrapper> connections = new();
        DoorWrapper initialSearchDoor = SelectDoor(door);
        if (initialSearchDoor == null)
        {
            return false;
        }
        door.pendingDeletion = true;
        HasConnectingDoorRecursiveDFS(initialSearchDoor, connections);
        door.pendingDeletion = false;
        return connections.Count == doors.Count - 1;
    }

    private DoorWrapper SelectDoor(DoorWrapper door)
    {
        foreach (RoomWrapper connectingRoom in door.connectingRooms)
        {
            foreach (DoorWrapper connectingDoors in door.connectingRooms[0].doors)
            {
                if (connectingDoors != door)
                {
                    return connectingDoors;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// O(n) recursive operation that adds all connecting rooms in a Depth First Search.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="connections"></param>
    private void HasConnectingDoorRecursiveDFS(DoorWrapper door, HashSet<DoorWrapper> connections)
    {
        if (!connections.Contains(door) && !door.pendingDeletion)
        {
            connections.Add(door);
            foreach (RoomWrapper room in door.connectingRooms)
            {
                foreach (DoorWrapper connectingDoor in room.doors)
                {
                    HasConnectingDoorRecursiveDFS(connectingDoor, connections);
                }
            }
        }
    }

    private void RemoveDoor(List<DoorWrapper> doors, DoorWrapper door)
    {
        foreach(RoomWrapper room in door.connectingRooms)
        {
            room.doors.Remove(door);
        }
        doors.Remove(door);
    }

    private void DrawConnection(Vector2 start, Vector2 end)
    {
        DebugDrawingBatcher.GetInstance("Connections").BatchCall(() => Debug.DrawLine(new Vector3(start.x, 0, start.y), new Vector3(end.x, 0, end.y), Color.magenta, 0));
    }

    private void WriteDebug(object message)
    {
        if (writeDebug)
        {
            Debug.Log(message);
        }
    }
}
