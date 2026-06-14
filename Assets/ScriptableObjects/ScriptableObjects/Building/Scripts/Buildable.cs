
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildableData", menuName = "Building/BuildableData")]
public class BuildableData : ScriptableObject
{
    public string id;
    public GameObject finalPrefab;
    public GameObject previewPrefab;
    public Vector3 placementOffset = Vector3.zero;
    public float maxSlopeDegrees = 20f;
    public LayerMask groundMask;

    [Header("Koszty budowy")]
    public List<BuildCost> costs = new List<BuildCost>();
}

[System.Serializable]
public class BuildCost
{
    public ItemObject item;   // referencja do ScriptableObject konkretnego itemu (ma swoje id)
    public int amount = 1;    // ile sztuk trzeba
}
