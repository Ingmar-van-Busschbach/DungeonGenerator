using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConnectionGenerator))]
public class ConnectionGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ConnectionGenerator connectionGenerator = (ConnectionGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate Connections"))
            {
                connectionGenerator.StartGeneration();
            }
        }
    }
}