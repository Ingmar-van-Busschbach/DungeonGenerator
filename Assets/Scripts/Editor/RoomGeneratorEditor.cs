using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomGenerator))]
public class RoomGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RoomGenerator dungeonGenerator = (RoomGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate Rooms"))
            {
                dungeonGenerator.StartGeneration();
            }
        }
    }
}
