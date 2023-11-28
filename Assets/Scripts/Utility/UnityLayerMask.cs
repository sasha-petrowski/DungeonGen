using System;

public enum UnityLayer
{
    Defaut = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Terrain = 3,
    Water = 4,
    UI = 5,
    Void = 6,
    Wall = 7,
    Entity = 8,
    AirEntity = 9,
}

[Flags]
public enum UnityLayerMask
{
    Defaut = 1 << (int)UnityLayer.Defaut,
    TransparentFX = 1 << (int)UnityLayer.TransparentFX,
    IgnoreRaycast = 1 << (int)UnityLayer.IgnoreRaycast,
    Terrain = 1 << (int)UnityLayer.Terrain,
    Water = 1 << (int)UnityLayer.Water,
    UI = 1 << (int)UnityLayer.UI,
    Void = 1 << (int)UnityLayer.Void,
    Wall = 1 << (int)UnityLayer.Wall,
    Entity = 1 << (int)UnityLayer.Entity,
    AirEntity = 1 << (int)UnityLayer.AirEntity,
}