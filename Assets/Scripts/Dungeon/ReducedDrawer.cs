using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DungeonWrapper))]
public class ReducedDrawer : ProceduralGenerator
{
    [Header("Drawing Settings")]
    [SerializeField] private Color roomColor = Color.red;
    [SerializeField] private Color doorColor = Color.blue;

    private DungeonWrapper dungeonWrapper;

    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }

    public void StartDrawing()
    {
        ClearDrawingBatchers();
        StopAllCoroutines();
        StartCoroutine(DrawRooms());
    }

    private void ClearDrawingBatchers()
    {
        DebugDrawingBatcher.GetInstance("Rooms").ClearAllBatchedCalls();
        DebugDrawingBatcher.GetInstance("Leafs").ClearAllBatchedCalls();
        DebugDrawingBatcher.GetInstance("Doors").ClearAllBatchedCalls();
        DebugDrawingBatcher.GetInstance("Connections").ClearAllBatchedCalls();
    }

    private IEnumerator DrawRooms()
    {
        Debug.Log("Starting redrawing of dungeon...");
        foreach (RoomWrapper room in dungeonWrapper.reducedRooms)
        {
            if (executionDelay > 0)
            {
                yield return new WaitForSeconds(executionDelay);
            }
            DrawRoom(room, roomColor, "Leafs");
        }
        foreach(DoorWrapper door in dungeonWrapper.reducedDoors)
        {
            if (executionDelay > 0)
            {
                yield return new WaitForSeconds(executionDelay);
            }
            DrawDoor(door, doorColor, "Doors");
        }
        Debug.Log("redrawing of dungeon completed...");
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.RoomsDrawn));
    }

    private void DrawRoom(RoomWrapper currentRoom, Color color, string debugDrawer = "default")
    {
        //Debug Drawing Batcher must use a value instead of a reference, so it cannot use currentBTEntry.room data as that is a reference type.
        RectInt room = currentRoom.room;
        DebugDrawingBatcher.GetInstance(debugDrawer).BatchCall(() => AlgorithmsUtils.DebugRectInt(room, color));
    }

    private void DrawDoor(DoorWrapper currentDoor, Color color, string debugDrawer = "default")
    {
        //Debug Drawing Batcher must use a value instead of a reference, so it cannot use currentDoor data as that is a reference type.
        RectInt door = currentDoor.door;
        DebugDrawingBatcher.GetInstance(debugDrawer).BatchCall(() => AlgorithmsUtils.DebugRectInt(door, color));
    }
}
