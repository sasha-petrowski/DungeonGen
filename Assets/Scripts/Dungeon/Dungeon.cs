using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dungeon : MonoBehaviour
{
    public int Seed;
    private System.Random _seedRandom;

    [SerializeField]
    protected PlayerEntity _playerPrefab;
    protected PlayerEntity _player;

    [Header("Dungeon")]
    public int Width = 64;
    public int Height = 64;

    public Tile[,] Tiles;

    public bool Generated = false;

    public TileData TileData;
    public Tilemap Voidmap;
    public Tilemap Tilemap;
    public Tilemap Overmap;

    private void OnValidate()
    {
        Generated = false;
    }

    private void Start()
    {
        if(Seed == 0)
        {
            Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        Generate();

        CreateTilemap();

        SpawnPlayer();
    }

    protected virtual void SpawnPlayer()
    {
        _player = GameObject.Instantiate(_playerPrefab.gameObject).GetComponent<PlayerEntity>();
    }

    public virtual void Generate()
    {
        Tilemap.ClearAllTiles();
        Overmap.ClearAllTiles();
        Voidmap.ClearAllTiles();

        _seedRandom = new System.Random(Seed);

        Tiles = new Tile[Width, Height];

        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                Tiles[x,y] = new Tile(x, y, this);
            }
        }
    }

    public int Random(int min, int max)
    {
        return min + _seedRandom.Next(max - min);
    }
    public float Random(float min, float max)
    {
        return min + ((float)_seedRandom.NextDouble() % (max - min));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(Width / 2f, Height / 2f), new Vector3(Width, Height));
    }

    public bool TryGetTile(int x, int y, out Tile tile)
    {
        if (x < 0 | y < 0 | x >= Width | y >= Height)
        {
            tile = null;
            return false;
        }
        else
        {
            tile = Tiles[x, y];
            return true;
        }
    }

    public void CreateTilemap()
    {
        Tilemap.ClearAllTiles();
        Overmap.ClearAllTiles();
        Voidmap.ClearAllTiles();

        #region for each tile split into 3x3 and fill depending on neighbors
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {

                #region Get all directions
                bool center = TryGetTile(x, y, out Tile centerTile) && centerTile.DjikstraMap != -1;

                bool bottom = TryGetTile(x, y - 1, out Tile bottomTile) && bottomTile.DjikstraMap != -1;
                bool top = TryGetTile(x, y + 1, out Tile topTile) && topTile.DjikstraMap != -1;
                bool right = TryGetTile(x + 1, y, out Tile rightTile) && rightTile.DjikstraMap != -1;
                bool left = TryGetTile(x - 1, y, out Tile leftTile) && leftTile.DjikstraMap != -1;


                bool bottomLeft = TryGetTile(x - 1, y - 1, out Tile bottomLeftTile) && bottomLeftTile.DjikstraMap != -1;
                bool topLeft = TryGetTile(x - 1, y + 1, out Tile topLeftTile) && topLeftTile.DjikstraMap != -1;
                bool bottomRight = TryGetTile(x + 1, y - 1, out Tile bottomRightTile) && bottomRightTile.DjikstraMap != -1;
                bool topRight = TryGetTile(x + 1, y + 1, out Tile topRightTile) && topRightTile.DjikstraMap != -1;

                if (!(center | bottom | top | right | left | bottomLeft | topLeft | bottomRight | topRight)) continue;

                if (center && centerTile.Feature != null)
                {
                    if (bottom && !(bottomTile.Feature == centerTile.Feature || (bottomTile.Feature != null && bottomTile.Feature.Links.Contains(centerTile.Feature)))) bottom = false;
                    if (top && !(topTile.Feature == centerTile.Feature || (topTile.Feature != null && topTile.Feature.Links.Contains(centerTile.Feature)))) top = false;
                    if (left && !(leftTile.Feature == centerTile.Feature || (leftTile.Feature != null && leftTile.Feature.Links.Contains(centerTile.Feature)))) left = false;
                    if (right && !(rightTile.Feature == centerTile.Feature || (rightTile.Feature != null && rightTile.Feature.Links.Contains(centerTile.Feature)))) right = false;

                    bottomLeft = bottomLeft & bottom & left;
                    topLeft = topLeft & top & left;
                    bottomRight = bottomRight & bottom & right;
                    topRight = topRight & top & right;

                    if (bottomLeft && !(bottomLeftTile.Feature == centerTile.Feature || (bottomLeftTile.Feature != null && (bottomLeftTile.Feature == leftTile.Feature || bottomLeftTile.Feature.Links.Contains(leftTile.Feature)) && (bottomLeftTile.Feature == bottomTile.Feature || bottomLeftTile.Feature.Links.Contains(bottomTile.Feature))))) bottomLeft = false;
                    if (topLeft && !(topLeftTile.Feature == centerTile.Feature || (topLeftTile.Feature != null && (topLeftTile.Feature == leftTile.Feature || topLeftTile.Feature.Links.Contains(leftTile.Feature)) && (topLeftTile.Feature == topTile.Feature || topLeftTile.Feature.Links.Contains(topTile.Feature))))) topLeft = false;
                    if (bottomRight && !(bottomRightTile.Feature == centerTile.Feature || (bottomRightTile.Feature != null && (bottomRightTile.Feature == rightTile.Feature || bottomRightTile.Feature.Links.Contains(rightTile.Feature)) && (bottomRightTile.Feature == bottomTile.Feature || bottomRightTile.Feature.Links.Contains(bottomTile.Feature))))) bottomRight = false;
                    if (topRight && !(topRightTile.Feature == centerTile.Feature || (topRightTile.Feature != null && (topRightTile.Feature == rightTile.Feature || topRightTile.Feature.Links.Contains(rightTile.Feature)) && (topRightTile.Feature == topTile.Feature || topRightTile.Feature.Links.Contains(topTile.Feature))))) topRight = false;

                }
                #endregion

                #region Decalre positions

                Vector3Int centerPos = new Vector3Int(x * 3, y * 3);

                Vector3Int leftPos = new Vector3Int(x * 3 - 1, y * 3);
                Vector3Int rightPos = new Vector3Int(x * 3 + 1, y * 3);
                Vector3Int bottomPos = new Vector3Int(x * 3, y * 3 - 1);
                Vector3Int topPos = new Vector3Int(x * 3, y * 3 + 1);

                Vector3Int bottomLeftPos = new Vector3Int(x * 3 - 1, y * 3 - 1);
                Vector3Int bottomRightPos = new Vector3Int(x * 3 + 1, y * 3 - 1);
                Vector3Int topLeftPos = new Vector3Int(x * 3 - 1, y * 3 + 1);
                Vector3Int topRightPos = new Vector3Int(x * 3 + 1, y * 3 + 1);

                #endregion

                #region Terrain tile
                if (center)
                {
                    //Center
                    if (centerTile.IsHole)
                    {
                        Voidmap.SetTile(centerPos, top ? TileData.Void : TileData.VoidBottom.GetTile(centerPos));
                    }
                    else
                    {
                        Tilemap.SetTile(centerPos, TileData.FloorFlat.GetTile(centerPos));
                    }

                    //Top
                    if (top & centerTile.IsHole)
                    {
                        Voidmap.SetTile(topPos, topTile != null && topTile.IsHole ? TileData.Void : TileData.VoidBottom.GetTile(topPos));
                    }
                    else
                    {
                        Tilemap.SetTile(topPos, top ? TileData.FloorFlat.GetTile(topPos) : TileData.Wall.GetTile(topPos));
                    }

                    //Bottom
                    if (bottom)
                    {
                        if (centerTile.IsHole)
                        {
                            Voidmap.SetTile(bottomPos, TileData.Void);
                        }
                        else
                        {
                            Tilemap.SetTile(bottomPos, TileData.FloorFlat.GetTile(bottomPos));
                        }
                    }
                    else
                    {
                        Overmap.SetTile(bottomPos, TileData.TopDown);
                    }

                    //Right
                    if (right)
                    {
                        //Right
                        if (centerTile.IsHole)
                        {
                            Voidmap.SetTile(rightPos, topRight ? rightTile != null && rightTile.IsHole ? TileData.Void : TileData.VoidRight.GetTile(rightPos) : TileData.VoidBottom.GetTile(rightPos));
                        }
                        else
                        {
                            Tilemap.SetTile(rightPos, TileData.FloorFlat.GetTile(rightPos));
                        }

                        //Top Right
                        if (top & topRight & centerTile.IsHole)
                        {
                            Voidmap.SetTile(topRightPos, topTile != null && topTile.IsHole ? (rightTile != null && !rightTile.IsHole) || (topRight && !topRightTile.IsHole) ? TileData.VoidRight.GetTile(topRightPos) : TileData.Void : TileData.VoidBottom.GetTile(topRightPos));
                        }
                        else
                        {
                            Tilemap.SetTile(topRightPos, (top & topRight ? centerTile.IsHole ? topTile != null && topTile.IsHole ? (rightTile != null && !rightTile.IsHole) || (topRight && !topRightTile.IsHole) ? TileData.VoidRight.GetTile(topRightPos) : TileData.Void : TileData.VoidBottom.GetTile(topRightPos) : TileData.FloorFlat.GetTile(topRightPos) : TileData.Wall.GetTile(topRightPos)));
                        }
                    }
                    else
                    {
                        //Right
                        Overmap.SetTile(rightPos, TileData.TopFlat);
                        //Top Right
                        Overmap.SetTile(topRightPos, TileData.TopFlat);
                    }

                    //Left
                    if (left)
                    {
                        //Left
                        if (centerTile.IsHole)
                        {
                            Voidmap.SetTile(leftPos, topLeft ? leftTile != null && leftTile.IsHole ? TileData.Void : TileData.VoidLeft.GetTile(leftPos) : TileData.VoidBottom.GetTile(leftPos));
                        }
                        else
                        {
                            Tilemap.SetTile(leftPos, TileData.FloorFlat.GetTile(leftPos));
                        }

                        //Top left
                        if (top & topLeft & centerTile.IsHole)
                        {
                            Voidmap.SetTile(topLeftPos, topTile != null && topTile.IsHole ? (leftTile != null && !leftTile.IsHole) || (topLeft && !topLeftTile.IsHole) ? TileData.VoidLeft.GetTile(topLeftPos) : TileData.Void : TileData.VoidBottom.GetTile(topLeftPos));
                        }
                        else
                        {
                            Tilemap.SetTile(topLeftPos, (top & topLeft ? TileData.FloorFlat.GetTile(topLeftPos) : TileData.Wall.GetTile(topLeftPos)));
                        }
                    }
                    else
                    {
                        //left
                        Overmap.SetTile(leftPos, TileData.TopFlat);
                        //Top Left
                        Overmap.SetTile(topLeftPos, TileData.TopFlat);
                    }

                    //Bottom Left
                    if (left & bottom & bottomLeft)
                    {
                        if (centerTile.IsHole)
                        {
                            Voidmap.SetTile(bottomLeftPos, leftTile != null && leftTile.IsHole ? TileData.Void : TileData.VoidLeft.GetTile(bottomLeftPos));
                        }
                        else
                        {
                            Tilemap.SetTile(bottomLeftPos, TileData.FloorFlat.GetTile(bottomLeftPos));
                        }
                    }
                    else
                    {
                        Overmap.SetTile(bottomLeftPos, (leftTile == null & !bottom) || (!bottomLeft && (bottomTile != null && (bottomLeftTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomLeftTile.Feature || bottomTile.Feature.Links.Contains(bottomLeftTile.Feature))))) ? TileData.TopDown : TileData.TopFlat);
                    }

                    //Bottom Right
                    if (right & bottom & bottomRight)
                    {
                        if (centerTile.IsHole)
                        {
                            Voidmap.SetTile(bottomRightPos, rightTile != null && rightTile.IsHole ? TileData.Void : TileData.VoidRight.GetTile(bottomRightPos));
                        }
                        else
                        {
                            Tilemap.SetTile(bottomRightPos, TileData.FloorFlat.GetTile(bottomRightPos));
                        }
                    }
                    else
                    {
                        Overmap.SetTile(bottomRightPos, (rightTile == null & !bottom) || (!bottomRight && (bottomTile != null && (bottomRightTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomRightTile.Feature || bottomTile.Feature.Links.Contains(bottomRightTile.Feature))))) ? TileData.TopDown : TileData.TopFlat);
                    }
                }
                #endregion
                #region Void tile
                else
                {
                    #region Sides
                    if (left)
                    {
                        Tilemap.SetTile(leftPos, TileData.VoidLeft.GetTile(leftPos));
                    }
                    if (right)
                    {
                        Tilemap.SetTile(rightPos, TileData.VoidRight.GetTile(rightPos));
                    }
                    if (bottom)
                    {
                        Overmap.SetTile(bottomPos, TileData.TopDown);
                        Overmap.SetTile(bottomLeftPos, bottomLeftTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomLeftTile.Feature || bottomTile.Feature.Links.Contains(bottomLeftTile.Feature)) ? TileData.TopDown : TileData.TopFlat);
                        Overmap.SetTile(bottomRightPos, bottomRightTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomRightTile.Feature || bottomTile.Feature.Links.Contains(bottomRightTile.Feature)) ? TileData.TopDown : TileData.TopFlat);
                    }
                    else
                    {
                        if (left)
                        {
                            Tilemap.SetTile(bottomLeftPos, TileData.VoidLeft.GetTile(bottomLeftPos));
                        }
                        if (right)
                        {
                            Tilemap.SetTile(bottomRightPos, TileData.VoidRight.GetTile(bottomRightPos));
                        }
                    }
                    if (top)
                    {
                        Tilemap.SetTile(topPos, TileData.VoidBottom.GetTile(topPos));
                        Tilemap.SetTile(topLeftPos, TileData.VoidBottom.GetTile(topLeftPos));
                        Tilemap.SetTile(topRightPos, TileData.VoidBottom.GetTile(topRightPos));
                    }
                    else
                    {
                        if (left)
                        {
                            Tilemap.SetTile(topLeftPos, TileData.VoidLeft.GetTile(topLeftPos));
                        }
                        if (right)
                        {
                            Tilemap.SetTile(topRightPos, TileData.VoidRight.GetTile(topRightPos));
                        }
                    }
                    #endregion

                    #region Corners
                    if ((!left & !top & topLeftTile != null) && topLeftTile.DjikstraMap != -1)
                    {
                        Tilemap.SetTile(topLeftPos, TileData.VoidLeft.GetTile(topLeftPos));
                    }
                    if ((!left & !bottom & bottomLeftTile != null) && bottomLeftTile.DjikstraMap != -1)
                    {
                        Tilemap.SetTile(bottomLeftPos, TileData.VoidLeft.GetTile(bottomLeftPos));
                    }
                    if ((!right & !top & topRightTile != null) && topRightTile.DjikstraMap != -1)
                    {
                        Tilemap.SetTile(topRightPos, TileData.VoidRight.GetTile(topRightPos));
                    }
                    if ((!right & !bottom & bottomRightTile != null) && bottomRightTile.DjikstraMap != -1)
                    {
                        Tilemap.SetTile(bottomRightPos, TileData.VoidRight.GetTile(bottomRightPos));
                    }
                    #endregion
                }
                #endregion
            }
        }
        #endregion

        TilemapCollider2D collider;

        if (Tilemap.TryGetComponent(out collider))
        {
            collider.ProcessTilemapChanges();
        }
        if (Overmap.TryGetComponent(out collider))
        {
            collider.ProcessTilemapChanges();
        }
    }
}
