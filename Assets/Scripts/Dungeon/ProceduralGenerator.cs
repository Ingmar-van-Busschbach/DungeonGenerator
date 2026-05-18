using UnityEngine;

public class ProceduralGenerator : MonoBehaviour
{
    [Header("Algorithm")]
    [Tooltip("Whether to start generating rooms on Start, or to wait for the Generate Room button to be pressed.")]
    public bool autoGenerate = true;
    [Tooltip("The time delay between generating rooms as part of the algorithm, in seconds.")]
    [Range(0, 0.1f)] public float executionDelay = 0.02f;
}
