using System.Collections;
using UnityEngine;
using static DungeonWrapper;

[RequireComponent(typeof(DungeonWrapper))]
[RequireComponent(typeof(RoomGenerator))]
public class TileMapGenerator : ProceduralGenerator
{
    private DungeonWrapper dungeonWrapper;
    private RoomGenerator roomGenerator;

    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
        roomGenerator = GetComponent<RoomGenerator>();
    }

    public void StartGeneration()
    {
        GenerateTileMap();
    }

    private void GenerateTileMap()
    {
        Debug.Log("Starting Tilemap Generation");
        dungeonWrapper.tileMap = new bool[(int)roomGenerator.dungeonSize.y,(int)roomGenerator.dungeonSize.x];
        foreach(RoomWrapper room in dungeonWrapper.reducedRooms)
        {
            AlgorithmsUtils.FillRectangleOutline(dungeonWrapper.tileMap, room.room, true);
        }
        foreach(DoorWrapper door in dungeonWrapper.reducedDoors)
        {
            AlgorithmsUtils.FillRectangle(dungeonWrapper.tileMap, door.door, false);
        }
        Debug.Log("Finished Tilemap Generation");
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonStatus.TileMapGenerated));
    }
}
