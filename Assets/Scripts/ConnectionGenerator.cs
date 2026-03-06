using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ConnectionGenerator : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("The ratio of smallest rooms that will be removed from the dungeon, if able.")]
    [Range(0, 1)] [SerializeField] private float removeRatio = 0.1f;

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
        dungeonWrapper.reducedRooms = new();
        foreach (RoomWrapper room in dungeonWrapper.rooms)
        {
            dungeonWrapper.reducedRooms.Add(room);
        }
        dungeonWrapper.reducedRooms.Sort((s1, s2) => s1.room.size.magnitude.CompareTo(s2.room.size.magnitude));
        dungeonWrapper.reducedRooms = ReduceRooms(dungeonWrapper.reducedRooms);
        foreach(RoomWrapper room in dungeonWrapper.reducedRooms)
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
        WriteDebug("Connection generation complete. " + dungeonWrapper.doors.Count + " rooms were removed successfullly, spanning " + (executionDelay > 0 ? ((Time.time - time)) + " seconds." : (stopwatch.ElapsedMilliseconds + "ms.")));
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.ConnectionsCompleted));
    }

    private List<RoomWrapper> ReduceRooms(List<RoomWrapper> rooms)
    {
        float removeAmount = rooms.Count * removeRatio;
        int removeCount = 0;
        for(int i = 0; i < rooms.Count; i++)
        {
            if(removeCount >= removeAmount)
            {
                WriteDebug("Successfully removed " + removeCount + " rooms due to being too small");
                break;
            }
            if (CanRemoveRoom(rooms[i], rooms[rooms.Count-1], rooms))
            {
                removeCount++;
                RemoveRoom(rooms, rooms[i]);
            }
        }
        return rooms;
    }

    private void RemoveRoom(List<RoomWrapper> rooms, RoomWrapper room)
    {
        for(int i = 0; i < room.doors.Count; i++)
        {
            for (int j = 0; j < room.doors[i].connectingRooms.Count; j++)
            {
                if (room.doors[i].connectingRooms[j] == room)
                {
                    continue;
                }
                room.doors[i].connectingRooms[j].doors.Remove(room.doors[i]);
            }
        }
        rooms.Remove(room);
    }

    private bool CanRemoveRoom(RoomWrapper room, RoomWrapper largestRoom, List<RoomWrapper> rooms)
    {
        List<RoomWrapper> connections = new();
        room.pendingDeletion = true;
        HasConnectingRoom(largestRoom, connections);
        room.pendingDeletion = false;
        Debug.Log(connections.Count + "/" + (rooms.Count-1));
        return connections.Count == rooms.Count - 1;
    }

    private void HasConnectingRoom(RoomWrapper room, List<RoomWrapper> connections)
    {
        if (!connections.Contains(room) && !room.pendingDeletion)
        {
            connections.Add(room);
            foreach(DoorWrapper door in room.doors)
            {
                foreach(RoomWrapper connectingRoom in door.connectingRooms)
                {
                    HasConnectingRoom(connectingRoom, connections);
                }
            }
        }
    }

    private void DrawConnection(Vector2 start, Vector2 end)
    {
        DebugDrawingBatcher.GetInstance("Connections").BatchCall(() => Debug.DrawLine(new Vector3(start.x, 0, start.y), new Vector3(end.x, 0, end.y), Color.green, 0));
    }

    private void WriteDebug(object message)
    {
        if (writeDebug)
        {
            Debug.Log(message);
        }
    }
}
