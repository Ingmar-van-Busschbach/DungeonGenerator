using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DungeonWrapper))]
public class AssetSpawner : ProceduralGenerator
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    private DungeonWrapper dungeonWrapper;
    private Vector3 offset = new Vector3(0.5f, 0, 0.5f);
    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }
    public void StartSpawning()
    {
        StartCoroutine(SpawnAssets());
    }
    private IEnumerator SpawnAssets()
    {
        GameObject parent = new GameObject("Generated Dungeon");
        foreach(RoomWrapper room in dungeonWrapper.reducedRooms)
        {
            if (executionDelay > 0)
            {
                yield return new WaitForSeconds(executionDelay);
            }
            GameObject roomParent = new GameObject("Room " + room.room.position.x + "," + room.room.position.y);
            roomParent.transform.parent = parent.transform;
            GameObject floorObject = Instantiate(floorPrefab, new Vector3(room.room.center.x, 0, room.room.center.y), Quaternion.identity, roomParent.transform);
            floorObject.transform.localScale = new Vector3(room.room.width, 1, room.room.height);
            floorObject.name = "Floor " + room.room.position.x + "," + room.room.position.y;
            for (int i = 0; i < room.room.width; i++)
            {
                AttemptSpawnWall(new Vector3(room.room.position.x + i, 0, room.room.position.y), roomParent.transform);
                AttemptSpawnWall(new Vector3(room.room.position.x + i, 0, room.room.yMax - 1), roomParent.transform);
            }
            for (int j = 0; j < room.room.height; j++)
            {
                AttemptSpawnWall(new Vector3(room.room.position.x, 0, room.room.position.y + j), roomParent.transform);
                AttemptSpawnWall(new Vector3(room.room.xMax - 1, 0, room.room.position.y + j), roomParent.transform);
            }
        }
        yield return null;
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.AssetsSpawned));
    }

    private void AttemptSpawnWall(Vector3 position, Transform parent)
    {
        GameObject wallObject = Instantiate(wallPrefab, position + offset, Quaternion.identity, parent);
        wallObject.name = "Wall " + position.x + "," + position.y;
    }
}
