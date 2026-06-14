using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public InventoryObject inventory;

    public bool Craft(CraftRecipe recipe)
    {
        if (!inventory.HasItems(recipe.costs))
            return false;

        inventory.ConsumeItems(recipe.costs);
        inventory.AddItem(recipe.resultItem, recipe.resultAmount);

        return true;
    }
}
