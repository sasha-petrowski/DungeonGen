using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class TilePicker : ScriptableObject
{
    public abstract TileBase GetTile(Vector3Int position);
}