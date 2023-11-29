using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Room : Feature
{
    public int Width;
    public int Height;

    public int x;
    public int y;

    public int Area { get; private set; }
    public Vector3 Center;
    public Vector3 TopLeft;
    public Vector3 Size;

    public Dungeon Dungeon;

    public int Index;

    public List<Room> LinkedRooms = new List<Room>();

    public override Vector2 GetPosition()
    {
        return Center;
    }

    public Room(int width, int height)
    {
        Width = width; 
        Height = height;

        Area = Width * Height;
    }

    public override string ToString()
    {
        return Index + "_" + base.ToString();
    }

    public override bool TryPlace(int x, int y, Dungeon dungeon)
    {
        #region Test Placement intersection and bounds
        if (x < 0 | y < 0 | x + Width > dungeon.Width | y + Height > dungeon.Height) return false;

        for (int X = x; X < x + Width; X++)
        {
            for (int Y = y; Y < y + Height; Y++)
            {
                if (dungeon.Tiles[X, Y].Feature != null) return false;
            }
        }
        #endregion

        this.x = x;
        this.y = y;
        Dungeon = dungeon;

        Center = new Vector3(x + Width / 2f, y + Height / 2f, 0) * 3;
        TopLeft = new Vector3(x, y + Height, 0) * 3;
        Size = new Vector3(Width, Height, 0) * 3;

        Place();

        return true;
    }
    protected void Place()
    {
        for (int X = x; X < x + Width; X++)
        {
            for (int Y = y; Y < y + Height; Y++)
            {
                Dungeon.Tiles[X, Y].Feature = this;
            }
        }
    }

    public IEnumerable<Tile> Neighbors()
    {
        foreach (Tile tile in BottomNeighbors())
        {
            yield return tile;
        }
        foreach (Tile tile in TopNeighbors())
        {
            yield return tile;
        }
        foreach (Tile tile in LeftNeighbors())
        {
            yield return tile;
        }
        foreach (Tile tile in RightNeighbors())
        {
            yield return tile;
        }
    }
    public IEnumerable<Tile> BottomNeighbors()
    {
        if (y - 1 < 0) yield break;

        for (int X = x; X < x + Width; X++)
        {
            yield return Dungeon.Tiles[X, y - 1];
        }
    }
    public IEnumerable<Tile> TopNeighbors()
    {
        if (y + Height >= Dungeon.Height) yield break;

        for (int X = x; X < x + Width; X++)
        {
            yield return Dungeon.Tiles[X, y + Height];
        }
    }
    public IEnumerable<Tile> LeftNeighbors()
    {
        if (x - 1 < 0) yield break;

        for (int Y = y; Y < y + Height; Y++)
        {
            yield return Dungeon.Tiles[x - 1, Y];
        }
    }
    public IEnumerable<Tile> RightNeighbors()
    {
        if (x + Width >= Dungeon.Width) yield break;

        for (int Y = y; Y < y + Height; Y++)
        {
            yield return Dungeon.Tiles[x + Width, Y];
        }
    }
}
