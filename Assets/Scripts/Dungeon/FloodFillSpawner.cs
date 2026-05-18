using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloodFillSpawner : ProceduralGenerator
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int objectsPerStep;
    private Vector3 offset = new Vector3(0.5f, 0, 0.5f);

    private DungeonWrapper dungeonWrapper;
    private int currentCount;
    private GameObject parent;

    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }

    public void StartGeneration()
    {
        StartCoroutine(FloodFillFloor());
    }

    private IEnumerator FloodFillFloor()
    {
        //Start execution timer
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        float time = Time.time;
        Debug.Log("Starting flood filling of floor...");
        if (parent != null)
        {
            Destroy(parent);
        }
        parent = new GameObject("Floor");
        int2 position = new int2(dungeonWrapper.reducedRooms[0].room.center);
        HashSet<int2> done = new();
        yield return StartCoroutine(SpawnFloorBFS(position, done));
        Debug.Log("Flood filling of floor completed, spanning " + (executionDelay > 0 ? ((Time.time - time)) + " seconds." : (stopwatch.ElapsedMilliseconds + "ms.")));
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.FloorSpawned));
    }

    private IEnumerator SpawnFloorBFS(int2 startPosition, HashSet<int2> done)
    {
        Queue<int2> queue = new Queue<int2>();
        queue.Enqueue(startPosition);
        while (queue.Count > 0)
        {
            int2 position = queue.Dequeue();
            if (done.Contains(position))
            {
                continue;
            }
            currentCount++;
            done.Add(position);
            GameObject floor = Instantiate(prefab, new Vector3(position.x, 0, position.y) + offset, Quaternion.identity, parent.transform);
            if (executionDelay > 0 && currentCount == objectsPerStep)
            {
                currentCount = 0;
                yield return new WaitForSeconds(executionDelay);
            }
            if (position.x < dungeonWrapper.tileMap.GetLength(1))
            {
                int2 right = new int2(position.x + 1, position.y);
                if (!dungeonWrapper.tileMap[right.y, right.x] && !done.Contains(right))
                {
                    queue.Enqueue(right);
                }
            }
            if (position.x > 0)
            {
                int2 left = new int2(position.x - 1, position.y);
                if (!dungeonWrapper.tileMap[left.y, left.x] && !done.Contains(left))
                {
                    queue.Enqueue(left);
                }
            }
            if (position.y < dungeonWrapper.tileMap.GetLength(0))
            {
                int2 up = new int2(position.x, position.y + 1);
                if (!dungeonWrapper.tileMap[up.y, up.x] && !done.Contains(up))
                {
                    queue.Enqueue(up);
                }
            }
            if (position.y > 0)
            {
                int2 down = new int2(position.x, position.y - 1);
                if (!dungeonWrapper.tileMap[down.y, down.x] && !done.Contains(down))
                {
                    queue.Enqueue(down);
                }
            }
        }
        
    }
}

public struct int2
{
    public int x;
    public int y;

    public int2(Vector2 input)
    {
        x = (int)input.x;
        y = (int)input.y;
    }
    public int2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
