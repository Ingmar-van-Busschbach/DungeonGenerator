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
            if (dungeonWrapper.gameObject.TryGetComponent(out RoomGenerator dungeonGenerator))
            {
                if (GUILayout.Button("Generate Rooms"))
                {
                    dungeonGenerator.StartGeneration();
                }
            }
            if (dungeonWrapper.gameObject.TryGetComponent(out DoorGenerator doorGenerator))
            {
                if (GUILayout.Button("Generate Doors"))
                {
                    doorGenerator.StartGeneration();
                }
            }
            if (dungeonWrapper.gameObject.TryGetComponent(out ConnectionGenerator connectionGenerator))
            {
                if (GUILayout.Button("Generate Connections"))
                {
                    connectionGenerator.StartGeneration();
                }
            }
        }
    }
}
