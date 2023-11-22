using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public abstract class Feature
{
    public List<Feature> Links = new List<Feature>();
    public abstract Vector2 GetPosition();
    public abstract bool TryPlace(int x, int y, Dungeon dungeon);
    public virtual bool TryPlace(Tile tile, Dungeon dungeon)
    {
        return TryPlace(tile.x, tile.y, dungeon);
    }
}