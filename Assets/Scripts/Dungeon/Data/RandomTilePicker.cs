using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Tile Picker/Random")]
public class RandomTilePicker : TilePicker
{
    public List<TileBase> Tiles;
    public override TileBase GetTile(Vector3Int position)
    {
        return Tiles[Random.Range(0, Tiles.Count)];
    }
}