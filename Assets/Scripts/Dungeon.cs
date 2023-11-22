using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dungeon : MonoBehaviour
{
    public int Seed;
    private System.Random _seedRandom;

    [Header("Dungeon")]
    public int Width = 64;
    public int Height = 64;

    public Tile[,] Tiles;

    public bool Generated = false;

    public TileData TileData;
    public Tilemap Tilemap;

    private void OnValidate()
    {
        Generated = false;
    }

    public virtual void Generate()
    {
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

    public virtual void Next() { }

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

                #region Center
                if (center)
                {
                    Tilemap.SetTile(new Vector3Int(x * 3, y * 3, 0), TileData.FloorFlat.GetTile(x * 3, y * 3));
                }
                else
                {
                    if (bottom)
                    {
                        Tilemap.SetTile(new Vector3Int(x * 3, y * 3 - 1, 0), TileData.TopDown);
                        Tilemap.SetTile(new Vector3Int(x * 3 - 1, y * 3 - 1, 0), bottomLeftTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomLeftTile.Feature || bottomTile.Feature.Links.Contains(bottomLeftTile.Feature)) ? TileData.TopDown : TileData.TopFlat);
                        Tilemap.SetTile(new Vector3Int(x * 3 + 1, y * 3 - 1, 0), bottomRightTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomRightTile.Feature || bottomTile.Feature.Links.Contains(bottomRightTile.Feature)) ? TileData.TopDown : TileData.TopFlat);
                    }
                    continue;
                }
                #endregion

                #region Top

                Tilemap.SetTile(new Vector3Int(x * 3, y * 3 + 1, 0), top ? TileData.FloorFlat.GetTile(x * 3, y * 3 + 1) : TileData.Wall.GetTile(x * 3, y * 3 + 1));

                #endregion
                #region Bottom

                Tilemap.SetTile(new Vector3Int(x * 3, y * 3 - 1, 0), bottom ? TileData.FloorFlat.GetTile(x * 3, y * 3 - 1) : TileData.TopDown);

                #endregion
                #region Right

                Tilemap.SetTile(new Vector3Int(x * 3 + 1, y * 3, 0), right ? TileData.FloorFlat.GetTile(x * 3 + 1, y * 3) : TileData.TopFlat);

                #endregion
                #region Left

                Tilemap.SetTile(new Vector3Int(x * 3 - 1, y * 3, 0), left ? TileData.FloorFlat.GetTile(x * 3 - 1, y * 3) : TileData.TopFlat);

                #endregion

                #region Top Left

                Tilemap.SetTile(new Vector3Int(x * 3 - 1, y * 3 + 1, 0), left ? (top & topLeft ? TileData.FloorFlat.GetTile(x * 3 - 1, y * 3 + 1) : TileData.Wall.GetTile(x * 3 - 1, y * 3 + 1)) : TileData.TopFlat);

                #endregion
                #region Top Right

                Tilemap.SetTile(new Vector3Int(x * 3 + 1, y * 3 + 1, 0), right ? (top & topRight ? TileData.FloorFlat.GetTile(x * 3 + 1, y * 3 + 1) : TileData.Wall.GetTile(x * 3 + 1, y * 3 + 1)) : TileData.TopFlat);

                #endregion
                #region Bottom Left

                Tilemap.SetTile(new Vector3Int(x * 3 - 1, y * 3 - 1, 0), left & bottom & bottomLeft ? TileData.FloorFlat.GetTile(x * 3 - 1, y * 3 - 1) : (!bottomLeft && (bottomTile != null && (bottomLeftTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomLeftTile.Feature || bottomTile.Feature.Links.Contains(bottomLeftTile.Feature))) || !bottom)) ? TileData.TopDown : TileData.TopFlat);

                #endregion
                #region Bottom Right

                Tilemap.SetTile(new Vector3Int(x * 3 + 1, y * 3 - 1, 0), right & bottom & bottomRight ? TileData.FloorFlat.GetTile(x * 3 + 1, y * 3 - 1) : (!bottomRight && (bottomTile != null && (bottomRightTile != null && (bottomTile.Feature == null || bottomTile.Feature == bottomRightTile.Feature || bottomTile.Feature.Links.Contains(bottomRightTile.Feature))) || !bottom)) ? TileData.TopDown : TileData.TopFlat);

                #endregion

            }
        }
    }
}
