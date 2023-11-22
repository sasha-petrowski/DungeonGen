using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Pathfinding
{
    public static readonly int[] directionX = new int[] { 0, 1, 0, -1 };
    public static readonly int[] directionY = new int[] { 1, 0, -1, 0 };

    public static bool Astar(Dungeon dungeon, Tile start, Tile end, out Dictionary<Tile, int> visited)
    {
        visited = new Dictionary<Tile, int>();

        if (start == null || end == null) return false;

        visited.Add(start, 0);

        if (start == end) return true;


        LinkedList<Tile> toVisit = new LinkedList<Tile>();
        LinkedList<int> visitWeights = new LinkedList<int>();

        toVisit.AddFirst(start);
        visitWeights.AddFirst(0);

        Tile tile;

        int nextX;
        int nextY;
        int nextTime;

        while (toVisit.First != null)
        {
            tile = toVisit.First.Value;

            toVisit.RemoveFirst();
            visitWeights.RemoveFirst();

            nextTime = visited[tile] + 1;

            for (int direction = 0; direction < 4; direction++)
            {
                nextX = tile.x + directionX[direction];
                nextY = tile.y + directionY[direction];

                if (nextX < 0 | nextY < 0 | nextX >= dungeon.Width | nextY >= dungeon.Height) continue;

                Tile neighbor = dungeon.Tiles[nextX, nextY];

                if (neighbor == end)
                {
                    visited.Add(neighbor, nextTime);
                    return true;
                }
                if (!(neighbor.IsEmpty || neighbor.Feature is Corridor) || visited.ContainsKey(neighbor)) continue;

                visited.Add(neighbor, nextTime);

                AddToVisit(neighbor, nextTime + ManhattanDistance(neighbor, end), toVisit, visitWeights);
            }
        }
        return false;
    }
    private static void AddToVisit(Tile tile, int weight, LinkedList<Tile> tiles, LinkedList<int> weights)
    {
        LinkedListNode<Tile> tileNode = tiles.First;
        LinkedListNode<int> weightNode = weights.First;

        while( tileNode != null )
        {
            if (weightNode.Value >= weight) break;

            tileNode = tileNode.Next;
            weightNode = weightNode.Next;
        }
        if( tileNode == null )
        {
            tiles.AddLast(tile);
            weights.AddLast(weight);
        }
        else
        {
            tiles.AddBefore(tileNode, tile);
            weights.AddBefore(weightNode, weight);
        }
    }
    public static List<Tile> Backtrack(Dungeon dungeon, Tile start, Tile end, Dictionary<Tile, int> visited)
    {
        List<Tile> path = new List<Tile>();

        Tile tile = end;

        Tile bestTile = null;
        int bestMove = int.MaxValue;

        int nextX;
        int nextY;

        while (tile != start)
        {
            path.Add(tile);


            for (int direction = 0; direction < 4; direction++)
            {
                nextX = tile.x + directionX[direction];
                nextY = tile.y + directionY[direction];

                if (nextX < 0 | nextY < 0 | nextX >= dungeon.Width | nextY >= dungeon.Height) continue;

                if (visited.TryGetValue(dungeon.Tiles[nextX, nextY], out int move) && move < bestMove)
                {
                    bestTile = dungeon.Tiles[nextX, nextY];
                    bestMove = move;
                }
            }

            tile = bestTile;
        }
        path.Add(tile);

        return path;
    }
    private static int ManhattanDistance(Tile a, Tile b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}