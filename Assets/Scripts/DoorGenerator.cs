using System.Collections;
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

    private System.Random numberGenerator;
    private DungeonWrapper dungeonWrapper;

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
        DebugDrawingBatcher.GetInstance("Doors").ClearAllBatchedCalls();
        StopAllCoroutines();
        StartCoroutine(WaitForRooms());
    }

    private IEnumerator WaitForRooms()
    {
        yield return new WaitUntil(()=>dungeonWrapper.dungeonStatus == DungeonWrapper.DungeonStatus.RoomsCompleted);
        StartCoroutine(GenerateDoors());
    }

    private IEnumerator GenerateDoors()
    {
        numberGenerator = new System.Random(seed);
        yield return null;
    }
}
