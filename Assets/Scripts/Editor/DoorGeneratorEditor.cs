using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorGenerator))]
public class DoorGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DoorGenerator doorGenerator = (DoorGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate Doors"))
            {
                doorGenerator.StartGeneration();
            }
        }
    }
}