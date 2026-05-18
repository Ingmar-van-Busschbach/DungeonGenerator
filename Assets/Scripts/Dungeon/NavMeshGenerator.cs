using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshGenerator : ProceduralGenerator
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    private DungeonWrapper dungeonWrapper;
    
    private void Start()
    {
        dungeonWrapper = GetComponent<DungeonWrapper>();
    }
    public void StartGeneration()
    {
        Debug.Log("Start building NavMesh...");
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh building complete.");
        StartCoroutine(dungeonWrapper.ChangeDungeonStatus(DungeonWrapper.DungeonStatus.NavMeshGenerated));
    }
}
