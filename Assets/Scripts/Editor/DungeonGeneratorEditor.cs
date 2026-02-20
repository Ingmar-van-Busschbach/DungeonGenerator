using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DungeonGenerator dungeonGenerator = (DungeonGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate Rooms"))
            {
                dungeonGenerator.StartGeneration();
            }
        }
    }
}
