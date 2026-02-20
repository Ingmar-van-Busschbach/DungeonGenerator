using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonWrapper))]
public class DungeonWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DungeonWrapper dungeonWrapper = (DungeonWrapper)target;
        if (dungeonWrapper.gameObject.TryGetComponent<DungeonGenerator>(out DungeonGenerator dungeonGenerator))
        {
            if (GUILayout.Button("Generate Dungeon"))
            {
                dungeonGenerator.StartGeneration();
            }
        }
    }
}
