using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI costsText;
    public Button craftButton;

    private CraftRecipe recipe;
    private CraftingSystem craftingSystem;
    private InventoryObject inventory;

    public void Init(CraftRecipe recipe, CraftingSystem system, InventoryObject inventory)
    {
        this.recipe = recipe;
        this.craftingSystem = system;
        this.inventory = inventory;

        itemNameText.text = recipe.resultItem.name;

        Refresh();

        craftButton.onClick.AddListener(OnCraftClicked);
        inventory.OnInventoryChanged += Refresh;
    }

    void Refresh()
    {
        costsText.text = "";

        bool canCraft = true;

        foreach (var cost in recipe.costs)
        {
            int owned = CountItem(cost.item);
            costsText.text += $"{owned}/{cost.amount} {cost.item.name}\n";

            if (owned < cost.amount)
                canCraft = false;
        }

        craftButton.interactable = canCraft;
    }

    int CountItem(ItemObject item)
    {
        int total = 0;
        foreach (var slot in inventory.Slots)
        {
            if (slot != null && slot.item == item)
                total += slot.amount;
        }
        return total;
    }

    void OnCraftClicked()
    {
        craftingSystem.Craft(recipe);
    }

    private void OnDestroy()
    {
        inventory.OnInventoryChanged -= Refresh;
    }
}
