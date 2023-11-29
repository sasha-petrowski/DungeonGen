using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Dungeon), true)]
public class DungeonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Dungeon dungeon = (Dungeon)target;
        if (dungeon == null)
        {
            DrawDefaultInspector();
            return;
        }

        if (GUILayout.Button("Randomize"))
        {
            dungeon.Seed = Random.Range(int.MinValue, int.MaxValue);
            dungeon.Generate();
        }
        if (GUILayout.Button("Generate"))
        {
            dungeon.Generate();
        }

        if (dungeon.Generated && GUILayout.Button("Tilemap"))
        {
            dungeon.CreateTilemap();
        }
        
        DrawDefaultInspector();
    }
}
