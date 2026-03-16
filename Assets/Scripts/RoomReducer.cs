using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DungeonWrapper))]
public class RoomReducer : MonoBehaviour
{
    [Header("Connection Settings")]
    [Tooltip("The ratio of smallest rooms that will be removed from the dungeon, if able.")]
    [Range(0, 1)] [SerializeField] private float removeRatio = 0.1f;
    [SerializeField] private bool removeLoops = true;
    [SerializeField] private SearchType searchType = SearchType.DepthFirstSearch;

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
        int removeCount = 0;
        ReduceRooms(dungeonWrapper.reducedRooms, ref removeCount);
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
        WriteDebug("Room reduction complete. " + removeCount + " rooms were removed successfullly, spanning " + (executionDelay > 0 ? ((Time.time - time)) + " seconds." : (stopwatch.ElapsedMilliseconds + "ms.")));
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.RoomsReduced));
    }

    private void ReduceRooms(List<RoomWrapper> rooms, ref int removeCount)
    {
        float removeAmount = rooms.Count * removeRatio;
        
        for(int i = 0; i < rooms.Count; i++)
        {
            if(removeCount >= removeAmount)
            {
                return;
            }
            if (CanRemoveRoom(rooms[i], rooms[rooms.Count-1], rooms))
            {
                removeCount++;
                RemoveRoom(rooms, rooms[i]);
            }
        }
    }

    private bool CanRemoveRoom(RoomWrapper room, RoomWrapper initialSearchRoom, List<RoomWrapper> rooms)
    {
        HashSet<RoomWrapper> connections = new();
        room.pendingDeletion = true;
        switch (searchType)
        {
            case SearchType.BreadthFirstSearch:
                HasConnectingRoomBFS(initialSearchRoom, connections);
                break;
            case SearchType.DepthFirstSearch:
                HasConnectingRoomDFS(initialSearchRoom, connections);
                break;
            case SearchType.RecursiveDepthFirstSearch:
                HasConnectingRoomRecursiveDFS(initialSearchRoom, connections);
                break;
        }
        room.pendingDeletion = false;
        return connections.Count == rooms.Count - 1;
    }

    /// <summary>
    /// O(n) iterative operation that adds all connecting rooms in a Breadth First Search.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="connections"></param>
    private void HasConnectingRoomBFS(RoomWrapper initialSearchRoom, HashSet<RoomWrapper> connections)
    {
        Queue<RoomWrapper> toDo = new();
        toDo.Enqueue(initialSearchRoom);
        connections.Add(initialSearchRoom);
        while (toDo.Count > 0)
        {
            RoomWrapper room = toDo.Dequeue();
            foreach (DoorWrapper door in room.doors)
            {
                foreach (RoomWrapper connectingRoom in door.connectingRooms)
                {
                    if (connections.Contains(connectingRoom) || connectingRoom.pendingDeletion)
                    {
                        continue;
                    }
                    toDo.Enqueue(connectingRoom);
                    connections.Add(connectingRoom);
                }
            }
        }
    }

    /// <summary>
    /// O(n) iterative operation that adds all connecting rooms in a Depth First Search.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="connections"></param>
    private void HasConnectingRoomDFS(RoomWrapper initialSearchRoom, HashSet<RoomWrapper> connections)
    {
        Stack<RoomWrapper> toDo = new();
        toDo.Push(initialSearchRoom);
        connections.Add(initialSearchRoom);
        while (toDo.Count > 0)
        {
            RoomWrapper room = toDo.Pop();
            foreach (DoorWrapper door in room.doors)
            {
                foreach (RoomWrapper connectingRoom in door.connectingRooms)
                {
                    if (connections.Contains(connectingRoom) || connectingRoom.pendingDeletion)
                    {
                        continue;
                    }
                    toDo.Push(connectingRoom);
                    connections.Add(connectingRoom);
                }
            }
        }
    }

    /// <summary>
    /// O(n) recursive operation that adds all connecting rooms in a Depth First Search.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="connections"></param>
    private void HasConnectingRoomRecursiveDFS(RoomWrapper room, HashSet<RoomWrapper> connections)
    {
        if (!connections.Contains(room) && !room.pendingDeletion)
        {
            connections.Add(room);
            foreach (DoorWrapper door in room.doors)
            {
                foreach (RoomWrapper connectingRoom in door.connectingRooms)
                {
                    HasConnectingRoomRecursiveDFS(connectingRoom, connections);
                }
            }
        }
    }

    private void RemoveRoom(List<RoomWrapper> rooms, RoomWrapper room)
    {
        for (int i = 0; i < room.doors.Count; i++)
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

    public enum SearchType { BreadthFirstSearch, DepthFirstSearch, RecursiveDepthFirstSearch }
}
