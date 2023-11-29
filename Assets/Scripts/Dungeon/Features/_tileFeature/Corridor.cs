using System.Collections.Generic;

public class Corridor : TileFeature
{
    private bool _isDoor = false;
    public bool IsDoor 
    {
        get { return _isDoor; }
        set 
        { 
            _isDoor = value; 
            if(_isDoor)
            {
                for (int i = 0; i < 4; i++)
                {
                    int x = Tile.x + Pathfinding.directionX[i];
                    int y = Tile.y + Pathfinding.directionY[i];

                    if (x < 0 | y < 0 | x >= Tile.Dungeon.Width | y >= Tile.Dungeon.Height) continue;

                    if(Links.Contains(Tile.Dungeon.Tiles[x, y].Feature))
                    {
                        Tile.Dungeon.Tiles[x, y].Reserved = true;
                    }
                }
            }
        }
    }
}