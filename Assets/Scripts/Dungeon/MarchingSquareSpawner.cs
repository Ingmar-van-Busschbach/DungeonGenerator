using System.Collections;
using UnityEngine;

public class MarchingSquareSpawner : ProceduralGenerator
{
    [SerializeField] private bool stepPerObject;

    [SerializeField] private GameObject[] prefabs = new GameObject[16];
    private Vector3 offset = new Vector3(1, 0, 1);

    private DungeonWrapper dungeonWrapper;

    private GameObject parent;

    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }

    public void StartGeneration()
    {
        StartCoroutine(SpawnMarchingSquares());
    }

    private IEnumerator SpawnMarchingSquares()
    {
        //Start execution timer
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        float time = Time.time;
        Debug.Log("Starting spawning of dungeon assets...");
        if (parent != null)
        {
            Destroy(parent);
        }
        parent = new GameObject("Walls");
        for (int i = 0; i < dungeonWrapper.tileMap.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < dungeonWrapper.tileMap.GetLength(1) - 1; j++)
            {
                int currentCase = (dungeonWrapper.tileMap[i, j + 1] ? 1 : 0) + (dungeonWrapper.tileMap[i + 1, j + 1] ? 2 : 0) + (dungeonWrapper.tileMap[i + 1, j] ? 4 : 0) + (dungeonWrapper.tileMap[i, j] ? 8 : 0);
                if (prefabs[currentCase] == null)
                {
                    continue;   
                }
                if (executionDelay > 0 && stepPerObject)
                {
                    yield return new WaitForSeconds(executionDelay);
                }
                GameObject wall = Instantiate(prefabs[currentCase], new Vector3(j, 0, i) + offset, Quaternion.identity, parent.transform);
                wall.name = "Wall [" + j + "," + i + "]";
            }
            if (executionDelay > 0 && !stepPerObject)
            {
                yield return new WaitForSeconds(executionDelay);
            }
        }
        Debug.Log("Spawning of dungeon assets completed, spanning " + (executionDelay > 0 ? ((Time.time - time)) + " seconds." : (stopwatch.ElapsedMilliseconds + "ms.")));
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.WallsSpawned));
    }
}
