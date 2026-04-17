using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class PuzzleTileData
{
    public PuzzleTileView.TileShape shape;
    [Range(0, 3)] public int rotation;
    public bool rotatable = true;
}
