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

        if (GUILayout.Button("test math"))
        {
            float angleA = 32;
            float angleB = 47;
            float angleC;

            float edgeC = 21;
            float edgeB;
            float edgeA;

            angleC = 180 - (32 + 47);

            //edgeA = (edgeC * Mathf.Sin(angleA * Mathf.Deg2Rad)) / Mathf.Sin(angleC * Mathf.Deg2Rad);
            edgeB = (edgeC * Mathf.Sin(angleB * Mathf.Deg2Rad)) / Mathf.Sin(angleC * Mathf.Deg2Rad);

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
