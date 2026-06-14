
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildCatalog", menuName = "Building/Build Catalog")]
public class BuildCatalog : ScriptableObject
{
    public List<BuildableData> entries = new List<BuildableData>();
}
