using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleLevelSet", menuName = "Puzzle/Puzzle Level Set")]
public class PuzzleLevelSet : ScriptableObject
{
    public string setId;
    public List<PuzzleLevelData> levels = new List<PuzzleLevelData>();
}