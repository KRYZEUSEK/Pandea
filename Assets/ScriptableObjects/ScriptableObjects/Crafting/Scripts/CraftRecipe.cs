using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftRecipe : ScriptableObject
{
    public ItemObject resultItem;
    public int resultAmount = 1;
    public List<BuildCost> costs;
}
