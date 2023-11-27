using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Tile Picker/Noise")]
public class NoiseTilePicker : TilePicker
{
    public List<TileBase> Tiles;
    public int Offset = 10000;
    [Min(1)]
    public int Octaves = 2;
    public float Scale = 10;
    [Range(0f, 1f)]
    public float Persistance = 0.75f;
    public float Continuity = 0.75f;

    [Header("Offset")]
    public float Multiplier = 1.1f;
    public float Minus = 0.05f;

    public int RandomBump = 1;

    public override TileBase GetTile(int x, int y)
    {
        x += Offset;
        y += Offset;

        float frequency = Scale;

        float noise = Mathf.PerlinNoise(x / frequency, y / frequency);

        for (int i = 1; i < Octaves; i++)
        {
            frequency *= Continuity;
            noise = noise * Persistance + Mathf.PerlinNoise(x / frequency, y / frequency) * (1 - Persistance);
        }

        return Tiles[Mathf.Clamp(Mathf.RoundToInt(Tiles.Count * ((noise - Minus) * Multiplier) + Random.Range(-RandomBump, RandomBump)), 0, Tiles.Count - 1)];
    }
}