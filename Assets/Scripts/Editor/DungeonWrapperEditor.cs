using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonWrapper))]
public class DungeonWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DungeonWrapper dungeonWrapper = (DungeonWrapper)target;
        if (Application.isPlaying)
        {
            if (dungeonWrapper.gameObject.TryGetComponent<DungeonGenerator>(out DungeonGenerator dungeonGenerator))
            {
                if (GUILayout.Button("Generate Rooms"))
                {
                    dungeonGenerator.StartGeneration();
                }
            }
            if (dungeonWrapper.gameObject.TryGetComponent<DoorGenerator>(out DoorGenerator doorGenerator))
            {
                if (GUILayout.Button("Generate Doors"))
                {
                    doorGenerator.StartGeneration();
                }
            }
        }
    }
}
