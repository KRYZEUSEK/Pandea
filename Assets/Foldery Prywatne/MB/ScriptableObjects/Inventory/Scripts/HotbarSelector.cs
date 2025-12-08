
using UnityEngine;

public class HotbarSelector : MonoBehaviour
{
    public InventoryObject inventory;
    public DisplayHotbar displayHotbar;

    public int CurrentIndex { get; private set; } = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SelectSlot((CurrentIndex - 1 + 5) % 5);
        else if (scroll < 0f) SelectSlot((CurrentIndex + 1) % 5);
    }


    public void SelectSlot(int index)
    {
        CurrentIndex = Mathf.Clamp(index, 0, 4);

        // Bezpieczne sprawdzenie
        var slot = inventory.Slots[CurrentIndex];
        if (slot == null || slot.item == null)
        {
            Debug.Log($"Wybrano pusty slot {CurrentIndex}");
        }
        else
        {
            Debug.Log($"Wybrano slot {CurrentIndex}: {slot.item.name}");
        }

        // Podœwietlenie w UI (zawsze dzia³a, nawet dla pustych slotów)
        if (displayHotbar != null)
            displayHotbar.SetSelectedIndex(CurrentIndex);
    }

}
