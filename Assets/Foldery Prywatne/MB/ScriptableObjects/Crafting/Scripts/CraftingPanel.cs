using UnityEngine;

public class CraftingPanel : MonoBehaviour
{
    public CraftRecipe[] recipes;
    public CraftingItemUI craftingItemPrefab;
    public Transform contentParent;
    public CraftingSystem craftingSystem;
    public InventoryObject inventory;

    void Start()
    {
        foreach (var recipe in recipes)
        {
            var ui = Instantiate(craftingItemPrefab, contentParent);
            ui.Init(recipe, craftingSystem, inventory);
        }
    }
}
