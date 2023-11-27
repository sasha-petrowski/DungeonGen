using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu()]
public class TileData : ScriptableObject
{
    public TilePicker FloorFlat;

    public TilePicker Wall;

    public TileBase TopFlat;
    public TileBase TopDown;

    public TilePicker VoidBottom;
    public TilePicker VoidLeft;
    public TilePicker VoidRight;

    public TileBase Void;
}