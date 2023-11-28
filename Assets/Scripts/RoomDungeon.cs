using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Delaunay;
using Unity.VisualScripting;
using System;

public class RoomDungeon : Dungeon
{
    [Header("Rooms")]
    public int MaxRoomWidth = 16;
    public int MinRoomWidth = 4;
    public int MaxRoomHeight = 16;
    public int MinRoomHeight = 4;

    public int MaxRooms = 16;
    public int MaxHoles = 16;
    [Min(1)]
    public int PlaceRoomRetry = 8;


    public int NearAngle = 15;

    public List<Room> Rooms = new List<Room>();

    private DRefPoint<Room>[] _dPoints = new DRefPoint<Room>[0];
    private DelaunayGraph _graph;

    public SpriteRenderer DjikstraMapRenderer;

    protected override void SpawnPlayer()
    {
        base.SpawnPlayer();
        _player.SpawnAt(Rooms[0].Center);
    }

    public override void Generate()
    {
        base.Generate();

        LinkedList<Room> rooms = new LinkedList<Room>();

        for(int i = 0; i < MaxRooms; i++)
        {
            Room room = new Room(Random(MinRoomWidth, MaxRoomWidth), Random(MinRoomHeight, MaxRoomHeight));
            #region Insert rooms depending on Area M²
            LinkedListNode<Room> before = rooms.First;
            while(before != null)
            {
                if(before.Value.Area >= room.Area)
                {
                    break;
                }
                before = before.Next;
            }
            if(before == null)
            {
                rooms.AddLast(room);
            }
            else
            {
                rooms.AddBefore(before, room);
            }
            #endregion
        }

        #region Allocate position to rooms
        LinkedListNode<Room> next = rooms.First;
        Rooms = new List<Room>(MaxRooms);

        int index = 0;
        while (next != null)
        {
            Room room = next.Value;
            next = next.Next;

            for(int i = 0; i < PlaceRoomRetry; i++)
            {
                if (room.TryPlace(Random(0, Width - room.Width), Random(0, Height - room.Height), this))
                {
                    room.Index = index;
                    index++;
                    Rooms.Add(room);
                    break;
                }
            }
        }
        #endregion

        #region Create Delaunay graph from rooms

        _dPoints = new DRefPoint<Room>[Rooms.Count];

        for(int i = 0;i < Rooms.Count; i++)
        {
            Room room = Rooms[i];
            _dPoints[i] = new DRefPoint<Room>(room.Center, room);
        }

        _graph = new DelaunayGraph(_dPoints, 0, Width * 3, 0, Height * 3);


        #endregion

        _graph.Complete();
        RemoveSimilarLinks();
        RemoveIntersections();
        LinkRooms();
        DjikstraMap();
        RenderDjikstraMap();
        AddTileFeatures();

        Generated = true;
    }

    private void AddTileFeatures()
    {
        int i = 0;
        bool valid;

        while(i < MaxHoles)
        {
            valid = true;
            Tile tile = Tiles[Random(0, Width), Random(0, Height)];

            if (tile.DjikstraMap < 1 | tile.Reserved) continue;

            for (int n = 0; n < 4; n++)
            {
                int x = tile.x + Pathfinding.directionX[n];
                int y = tile.y + Pathfinding.directionY[n];

                if ((x < 0 | y < 0 | x >= Width | y >= Height) || Tiles[x, y].Reserved)
                {
                    valid = false;
                }
            }

            if(valid)
            {
                tile.IsHole = true;
                i++;
            }
        }
    }

    private void RenderDjikstraMap()
    {

        Color[] colorMap = new Color[Width * Height];

        foreach (Tile tile in Tiles)
        {
            int value = tile.DjikstraMap * 4 % 765;

            if (tile.DjikstraMap == -1)
            {
                colorMap[tile.x + tile.y * Width] = Color.black;
            }
            else if (value < 255)
            {
                // vert > bleu
                colorMap[tile.x + tile.y * Width] = new Color(0, (255 - value) / 255f, value / 255f);
            }
            else if (value < 510)
            {
                // bleu > rouge
                colorMap[tile.x + tile.y * Width] = new Color((value - 255) / 255f, 0, (510 - value) / 255f);
            }
            else
            {
                // rouge > vert
                colorMap[tile.x + tile.y * Width] = new Color((765 - value) / 255f, (value - 510) / 255f, 0);
            }
        }

        Texture2D texture = new Texture2D(Width, Height);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        DjikstraMapRenderer.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0, 0), 1);
    }
    private void DjikstraMap()
    {
        HashSet<Tile> visited = new HashSet<Tile>();

        LinkedList<Tile> toVisit = new LinkedList<Tile>();

        Tile firstTile = Tiles[Mathf.RoundToInt(Rooms[0].x + Rooms[0].Width / 2), Mathf.RoundToInt(Rooms[0].y + Rooms[0].Height / 2)];
        firstTile.DjikstraMap = 0;

        toVisit.AddFirst(firstTile);

        int nextX;
        int nextY;
        int nextTime;
        Tile tile;

        while (toVisit.First != null)
        {
            tile = toVisit.First.Value;
            toVisit.RemoveFirst();

            nextTime = tile.DjikstraMap + 1;

            for (int direction = 0; direction < 4; direction++)
            {
                nextX = tile.x + Pathfinding.directionX[direction];
                nextY = tile.y + Pathfinding.directionY[direction];

                if (!TryGetTile(nextX, nextY, out Tile neighbor)) continue;

                if (neighbor.Feature == null | visited.Contains(neighbor)) continue;
                if (neighbor.Feature != tile.Feature)
                {
                    if (!((tile.Feature is Room && neighbor.Feature is Room) || (tile.Feature is Corridor tileCorridor && tileCorridor.Links.Contains(neighbor.Feature)) || (neighbor.Feature is Corridor corridor && corridor.Links.Contains(tile.Feature)))) continue;
                }

                visited.Add(neighbor);
                neighbor.DjikstraMap = nextTime;
                toVisit.AddLast(neighbor);
            }
        }
    }
    private void LinkRooms()
    {
        foreach (DPoint point in _dPoints)
        {
            if (point.IsVirtual()) continue;
            Room room = ((DRefPoint<Room>)point)?.Reference;
            
            if (room == null) continue;

            foreach (Tile neighbor in room.Neighbors())
            {
                if(neighbor.Feature is Room neighborRoom)
                {
                    neighbor.Reserved = true;
                    if (!room.Links.Contains(neighborRoom))
                    {
                        room.Links.Add(neighborRoom);
                        neighborRoom.Links.Add(room);
                    }
                }
            }

            foreach(DPoint other in point.Links)
            {
                if (other.IsVirtual()) continue;
                Room otherRoom = ((DRefPoint<Room>)other)?.Reference;

                if (otherRoom == null) continue;

                if (!room.Links.Contains(otherRoom) && !room.LinkedRooms.Contains(otherRoom))
                {

                    #region determine start and end tiles

                    IEnumerable<Tile> startTiles;
                    IEnumerable<Tile> endTiles;

                    Vector2 relative = otherRoom.Center - room.Center;

                    if (relative.x < 0)
                    {
                        if(Mathf.Abs(relative.y) > -relative.x)
                        {
                            if(relative.y < 0)
                            {
                                startTiles = room.BottomNeighbors();
                                endTiles = otherRoom.TopNeighbors();
                            }
                            else
                            {
                                startTiles = room.TopNeighbors();
                                endTiles = otherRoom.BottomNeighbors();
                            }
                        }
                        else
                        {
                            startTiles = room.LeftNeighbors();
                            endTiles = otherRoom.RightNeighbors();
                        }
                    }
                    else
                    {
                        if (Mathf.Abs(relative.y) > relative.x)
                        {
                            if (relative.y < 0)
                            {
                                startTiles = room.BottomNeighbors();
                                endTiles = otherRoom.TopNeighbors();
                            }
                            else
                            {
                                startTiles = room.TopNeighbors();
                                endTiles = otherRoom.BottomNeighbors();
                            }
                        }
                        else
                        {
                            startTiles = room.RightNeighbors();
                            endTiles = otherRoom.LeftNeighbors();
                        }
                    }

                    Tile startTile = GetPathTile(startTiles);
                    Tile endTile = GetPathTile(endTiles);
                    #endregion

                    if(CreatePath(startTile, endTile, room, otherRoom))
                    {
                        room.LinkedRooms.Add(otherRoom);
                        otherRoom.LinkedRooms.Add(room);
                    }
                }
            }
        }
    }
    private Tile GetPathTile(IEnumerable<Tile> tiles)
    {
        List<Tile> free = new List<Tile>();
        Tile firstCorridor = null;
        foreach (Tile tile in tiles)
        {
            if (tile.Feature is Corridor corridor)
            {
                if (corridor.IsDoor) return tile;

                if(firstCorridor == null) firstCorridor = tile;
            }
            if (tile.Feature == null) free.Add(tile);
        }
        if (free.Count > 0) return free[free.Count / 2];
        else if (firstCorridor != null) return firstCorridor;

        return null;
    }
    private void RemoveSimilarLinks()
    {
        float[] sqrMag = new float[16];

        foreach (DPoint point in _dPoints)
        {
            if (point.Links.Count > 1)
            {
                List<DPoint> toRemove = new List<DPoint>();

                for (int i = 0; i < point.Links.Count; i++)
                {
                    sqrMag[i] = Vector2.SqrMagnitude(point.Links[i].Position - point.Position);
                }
                for (int i = 0; i < point.Links.Count; i++)
                {
                    for (int n = i + 1; n < point.Links.Count; n++)
                    {
                        if (i == n) continue;

                        if (Mathf.Abs(Vector2.Angle(point.Links[i].Position - point.Position, point.Links[n].Position - point.Position)) <= NearAngle)
                        {
                            toRemove.Add(point.Links[sqrMag[i] > sqrMag[n] ? i : n]);
                        }
                    }
                }

                foreach (DPoint removePoint in toRemove)
                {
                    point.RemoveLink(removePoint);
                }
            }
        }
        
    }
    private void RemoveIntersections()
    {
        foreach (DTriangle triangle in _graph.Root.ExploreTree())
        {
            if (!triangle.IsLeaf) continue;

            RemoveIntersections(triangle.A, triangle.B);
            RemoveIntersections(triangle.B, triangle.C);
            RemoveIntersections(triangle.C, triangle.A);
        }
    }
    private void RemoveIntersections(DPoint a1, DPoint b1)
    {
        DPoint a2;
        DPoint a3;
        DPoint b2;
        for (int i = a1.Links.Count -1; i >= 0; i--)
        {
            a2 = a1.Links[i];

            if (a2 == b1) { continue; }

            for (int n = b1.Links.Count -1; n >= 0; n--)
            {
                b2 = b1.Links[n];

                if (a1 == b2) { continue; }
                if (a2 == b2) { continue; }

                if(DPoint.Intersect(a1, a2, b1, b2))
                {
                    if(Vector2.SqrMagnitude(a1.Position - a2.Position) > Vector2.SqrMagnitude(b1.Position - b2.Position))
                    {
                        a1.RemoveLink(a2);
                    }
                    else
                    {
                        b1.RemoveLink(b2);
                    }
                    break;
                }
            }

            for(int j = a2.Links.Count -1; j >= 0; j--)
            {
                a3 = a2.Links[j];

                if (a3 == b1) { continue; }

                for (int n = b1.Links.Count - 1; n >= 0; n--)
                {
                    b2 = b1.Links[n];

                    if (a2 == b2) { continue; }
                    if (a3 == b2) { continue; }

                    if (DPoint.Intersect(a2, a3, b1, b2))
                    {
                        if (Vector2.SqrMagnitude(a2.Position - a3.Position) > Vector2.SqrMagnitude(b1.Position - b2.Position))
                        {
                            a2.RemoveLink(a3);
                            break;
                        }
                        else
                        {
                            b1.RemoveLink(b2);
                        }
                    }
                }
            }
        }
    }
    private bool CreatePath(Tile start, Tile end, Room startRoom, Room endRoom)
    {
        if (Pathfinding.Astar(this, start, end, out Dictionary<Tile, int> visited))
        {
            List<Corridor> list = new List<Corridor>();

            List<Tile> tiles = Pathfinding.Backtrack(this, start, end, visited);

            Corridor previousCorridor = null;

            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                Corridor corridor = (Corridor)tile.Feature;

                if (corridor == null)
                {
                    corridor = new Corridor();

                    corridor.TryPlace(tile, this);
                }

                list.Add(corridor);

                if (i == 0)
                {
                    corridor.Links.Add(endRoom);
                    endRoom.Links.Add(corridor);

                    corridor.IsDoor = true;
                }
                if (i == tiles.Count - 1)
                {
                    corridor.Links.Add(startRoom);
                    startRoom.Links.Add(corridor);

                    if(previousCorridor != null)
                    {
                        corridor.Links.Add(previousCorridor);
                        previousCorridor.Links.Add(corridor);
                    }

                    corridor.IsDoor = true;
                }
                if(i > 0 & i < tiles.Count - 1)
                {
                    corridor.Links.Add(previousCorridor);
                    previousCorridor.Links.Add(corridor);
                }

                previousCorridor = corridor;
            }
            return true;
        }
        return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(Width / 2f * 3f, Height / 2f * 3f), new Vector3(Width * 3f, Height * 3f));

        Gizmos.color = Color.red;
        foreach (Room room in Rooms)
        {
            Handles.Label(room.TopLeft, $"_{room.Index}");
            Gizmos.DrawWireCube(room.Center, room.Size);
        }
        if(Tiles != null)
        {
            foreach (Tile tile in Tiles)
            {
                if (tile.Reserved)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(new Vector3((tile.x + 0.5f) * 3, (tile.y + 0.5f) * 3, 0), new Vector3(2.5f, 2.5f));
                }
                else if (tile.IsHole)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(new Vector3((tile.x + 0.5f) * 3, (tile.y + 0.5f) * 3, 0), new Vector3(2.5f, 2.5f));
                }
            }
        }

        if (_graph != null && _graph.Root != null)
        {
            Gizmos.color = Color.black;
            if(_graph.Finished)
            {
                foreach(DPoint point in _dPoints)
                {
                    DRefPoint<Room> refPoint = (DRefPoint<Room>)point;
                    if (refPoint == null) continue;

                    Gizmos.DrawWireSphere(point.Position, 1f);

                    foreach (DPoint link in point.Links)
                    {
                        if (link is DRefPoint<Room> refLink && !refLink.Reference.Links.Contains(refPoint.Reference) && !refLink.Reference.LinkedRooms.Contains(refPoint.Reference))
                        {
                            Gizmos.DrawLine(point.Position, (point.Position + link.Position) * 0.5f);
                        }
                    }
                }

                Gizmos.color = new Color(0, 1, 0, 0.2f);
                foreach (Room room in Rooms)
                {
                    foreach (Feature other in room.Links)
                    {
                        Gizmos.DrawLine(room.Center, (room.Center + (Vector3)other.GetPosition()) * 0.5f);
                    }
                }

                foreach(Tile tile in Tiles)
                {
                    if(tile.Feature is Corridor corridor)
                    {
                        if (corridor.IsDoor) Gizmos.color = Color.cyan;
                        else Gizmos.color = Color.blue;

                        Vector2 position = corridor.GetPosition();

                        foreach (Feature link in corridor.Links)
                        {
                            Gizmos.DrawLine(position, (position + link.GetPosition()) * 0.5f);

                        }
                    }
                }
            }
            else
            {
                foreach (DTriangle triangle in _graph.Root.ExploreTree())
                {
                    if (!triangle.IsLeaf) continue;

                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(triangle.A.Position, triangle.B.Position);
                    Gizmos.DrawLine(triangle.B.Position, triangle.C.Position);
                    Gizmos.DrawLine(triangle.C.Position, triangle.A.Position);

                    Gizmos.color = Color.gray;
                    if (triangle.AB != null) Gizmos.DrawLine(triangle.Position, (triangle.AB.Position + triangle.Position) * 0.5f);
                    if (triangle.BC != null) Gizmos.DrawLine(triangle.Position, (triangle.BC.Position + triangle.Position) * 0.5f);
                    if (triangle.CA != null) Gizmos.DrawLine(triangle.Position, (triangle.CA.Position + triangle.Position) * 0.5f);
                }
            }

            if(!_graph.Finished && _graph.Index < _dPoints.Length)
            {
                DPoint point = _dPoints[_graph.Index];

                DTriangle triangle = _graph.Root.GetLeaf(point);

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(point.Position, 1f);

                if (triangle != null)
                {
                    #region Highlight selected triangle
                    Gizmos.color = new Color(1, 1, 1, 0.5f);
                    Gizmos.DrawLine(triangle.A.Position, triangle.B.Position);
                    Gizmos.DrawLine(triangle.B.Position, triangle.C.Position);
                    Gizmos.DrawLine(triangle.C.Position, triangle.A.Position);
                    #endregion

                    Handles.Label(triangle.A.Position, "p.A");
                    Handles.Label(triangle.B.Position, "p.B");
                    Handles.Label(triangle.C.Position, "p.C");

                    DTriangle AB = triangle.AB;
                    DTriangle BC = triangle.BC;
                    DTriangle CA = triangle.CA;

                    Gizmos.color = Color.white;
                    if (AB != null)
                    {
                        Handles.Label(triangle.AB.Position, "t.AB");
                        Gizmos.DrawLine(AB.InnerA, AB.InnerB);
                        Gizmos.DrawLine(AB.InnerB, AB.InnerC);
                        Gizmos.DrawLine(AB.InnerC, AB.InnerA);
                    }
                    Gizmos.color = Color.red;
                    GizmoInsert(point, triangle.A, triangle.B, AB, out DTriangle ABb, out DTriangle ABc);

                    Handles.Label(ABb.Position, "t.AB.b");
                    if (ABb != ABc)
                        Handles.Label(ABc.Position, "t.AB.c");

                    Gizmos.color = Color.white;
                    if (BC != null)
                    {
                        Handles.Label(triangle.BC.Position, "t.BC");
                        Gizmos.DrawLine(BC.InnerA, BC.InnerB);
                        Gizmos.DrawLine(BC.InnerB, BC.InnerC);
                        Gizmos.DrawLine(BC.InnerC, BC.InnerA);
                    }
                    Gizmos.color = Color.green;
                    GizmoInsert(point, triangle.B, triangle.C, BC, out DTriangle BCb, out DTriangle BCc);

                    Handles.Label(BCb.Position, "t.BC.b");
                    if (BCb != BCc)
                        Handles.Label(BCc.Position, "t.BC.c");


                    Gizmos.color = Color.white;
                    if (CA != null)
                    {
                        Handles.Label(triangle.CA.Position, "t.CA");
                        Gizmos.DrawLine(CA.InnerA, CA.InnerB);
                        Gizmos.DrawLine(CA.InnerB, CA.InnerC);
                        Gizmos.DrawLine(CA.InnerC, CA.InnerA);
                    }
                    Gizmos.color = Color.blue;
                    GizmoInsert(point, triangle.C, triangle.A, CA, out DTriangle CAb, out DTriangle CAc);

                    Handles.Label(CAb.Position, "t.CA.b");
                    if (CAb != CAc)
                        Handles.Label(CAc.Position, "t.CA.c");


                }
            }
        }
    }

    private void GizmoInsert(DPoint a, DPoint b, DPoint c, DTriangle other, out DTriangle left, out DTriangle right)
    {
        #region Lawson flip
        if (other != null)
        {
            DPoint opposite = other.Opposite(b, c);
            Color previousColor = Gizmos.color;
            Vector2 center = (a.Position + opposite.Position) / 2;
            float distance = Vector2.Distance(center, a.Position);

            Gizmos.color = new Color(1, 1, 1, 0.25f);
            Gizmos.DrawWireSphere(center, distance);
            Gizmos.color = previousColor;

            if (distance < Vector2.Distance(center, b.Position) && distance < Vector2.Distance(center, c.Position))
            {
                Gizmos.DrawWireSphere(center, distance);
                // Lawson flip
                
                left  = new DTriangle(a, b, opposite);
                right = new DTriangle(a, opposite, c);

                Gizmos.DrawLine(left.InnerA, left.InnerB);
                Gizmos.DrawLine(left.InnerB, left.InnerC);
                Gizmos.DrawLine(left.InnerC, left.InnerA);

                Gizmos.DrawLine(right.InnerA, right.InnerB);
                Gizmos.DrawLine(right.InnerB, right.InnerC);
                Gizmos.DrawLine(right.InnerC, right.InnerA);
                /*
                left.CA = right;
                right.AB = left;

                DTriangle edgeLeft = other.GetEdge(opposite, b);
                left.BC = edgeLeft;
                edgeLeft?.AddEdge(opposite, b, left);

                DTriangle edgeRight = other.GetEdge(opposite, c);
                right.BC = edgeRight;
                edgeRight?.AddEdge(opposite, c, left);

                foreach (DTriangle parent in other.Parents)
                {
                    parent.Childs.Remove(other);
                    parent.Childs.Add(left);
                    parent.Childs.Add(right);

                    left.Parents.Add(parent);
                    right.Parents.Add(parent);
                }

                Childs.Add(left);
                Childs.Add(right);

                left.Parents.Add(this);
                right.Parents.Add(this);
                */
                return;

            }

        }
        #endregion
        #region Normal triangle

        left = new DTriangle(a, b, c);
        right = left;
        Gizmos.DrawLine(left.InnerA, left.InnerB);
        Gizmos.DrawLine(left.InnerB, left.InnerC);
        Gizmos.DrawLine(left.InnerC, left.InnerA);

        /*

        left.BC = other;
        other?.AddEdge(b, c, left);

        Childs.Add(left);
        left.Parents.Add(this);
        */

        #endregion

    }
}
