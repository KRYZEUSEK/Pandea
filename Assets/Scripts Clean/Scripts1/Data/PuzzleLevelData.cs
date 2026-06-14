using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleLevel", menuName = "Puzzle/Puzzle Level")]
public class PuzzleLevelData : ScriptableObject
{
    public string levelId;
    public int width = 5;
    public int height = 5;
    public int attempts = 3;

    [TextArea]
    public string notes;

    public List<PuzzleTileData> tiles = new List<PuzzleTileData>();
}
