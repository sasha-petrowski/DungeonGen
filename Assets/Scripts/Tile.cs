using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile
{
    public bool Reserved = false;
    public bool IsHole = false;

    public int x;
    public int y;

    public int DjikstraMap = -1;

    public bool IsEmpty => Feature == null;

    public Dungeon Dungeon;
    public Feature Feature;

    public Tile(int x, int y, Dungeon dungeon)
    {
        this.x = x;
        this.y = y;
        Dungeon = dungeon;
    }

    public override string ToString()
    {
        return $"Tile ({x}, {y}) : F.({Feature})";
    }
}
