
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInventory : MonoBehaviour
{
    public InventoryObject inventory;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public Transform hotbarParent;
    public Transform backpackParent;

    private bool isOpen = false;
    public int draggedSlotIndex = -1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
            if (isOpen) RefreshInventory();
        }
    }

    void OnEnable()
    {
        inventory.OnInventoryChanged += RefreshInventory;
    }

    void OnDisable()
    {
        inventory.OnInventoryChanged -= RefreshInventory;
    }


    void RefreshInventory()
    {
        foreach (Transform child in hotbarParent) Destroy(child.gameObject);
        foreach (Transform child in backpackParent) Destroy(child.gameObject);

        for (int i = 0; i < 5; i++)
        {
            CreateSlot(inventory.Slots[i], hotbarParent, i);
        }

        for (int i = 5; i < inventory.Slots.Length; i++)
        {
            CreateSlot(inventory.Slots[i], backpackParent, i);
        }
    }

    void CreateSlot(InventorySlot slot, Transform parent, int index)
    {
        GameObject ui;

        if (slot != null && slot.item != null && slot.item.uiPrefab != null)
        {
            ui = Instantiate(slot.item.uiPrefab, parent);
        }
        else
        {
            ui = Instantiate(slotPrefab, parent);
        }

        var text = ui.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = slot.amount > 0 ? slot.amount.ToString() : "";


        var slotUI = ui.GetComponent<InventorySlotUI>();
        if (slotUI != null)
        {
            slotUI.slotIndex = index;
            slotUI.displayInventory = this;
            slotUI.currentSlot = slot;
        }
        else
        {
            Debug.LogWarning($"Brak InventorySlotUI w obiekcie {ui.name}");
        }

    }

    public void StartDrag(int index)
    {
        draggedSlotIndex = index;
    }

    public void EndDrag()
    {
        draggedSlotIndex = -1;
        RefreshInventory(); // odœwie¿ po przeci¹ganiu
    }

    public void SwapSlots(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || fromIndex < 0 || toIndex < 0) return;

        var temp = inventory.Slots[fromIndex];
        inventory.Slots[fromIndex] = inventory.Slots[toIndex];
        inventory.Slots[toIndex] = temp;

        RefreshInventory();
    }
}
