using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFeature : Feature
{
    public override Vector2 GetPosition()
    {
        return new Vector2((Tile.x + 0.5f) * 3, (Tile.y + 0.5f) * 3);
    }

    public Tile Tile;
    public Dungeon Dungeon;

    public override bool TryPlace(int x, int y, Dungeon dungeon)
    {
        #region Test bounds and tile is empty
        if (x < 0 | y < 0 | x >= dungeon.Width | y >= dungeon.Height) return false;
        #endregion

        return TryPlace(dungeon.Tiles[x, y], dungeon);
    }
    public override bool TryPlace(Tile tile, Dungeon dungeon)
    {
        #region Test tile is empty
        if (! tile.IsEmpty) return false;
        #endregion

        Dungeon = dungeon;
        Tile = tile;
        Tile.Feature = this;
        Tile.Reserved = true;

        return true;
    }

    public override bool IsInside(Tile tile)
    {
        return Tile == tile;
    }
    public override bool IsEdge(Tile tile)
    {
        return Tile == tile;
    }
}
