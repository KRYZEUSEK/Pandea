
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayHotbar : MonoBehaviour
{
    public InventoryObject Inventory;
    public GameObject emptySlotPrefab;
    public int X_START = 0;
    public int X_SPACE = 100;

    // Zamiast InventorySlot -> int (indeks)
    Dictionary<int, GameObject> displayed = new Dictionary<int, GameObject>();

    private int selectedIndex = -1;

    void Start()
    {
        RefreshHotbar();
    }

    void Update()
    {
        RefreshHotbar();
    }

    void RefreshHotbar()
    {


        for (int i = 0; i < 5; i++)
        {
            var slot = Inventory.Slots[i];
            GameObject ui;

            // Jeśli slot ma przedmiot
            if (slot != null && slot.item != null)
            {
                // Jeśli w displayed jest pusty slot → usuń go
                if (displayed.ContainsKey(i))
                {
                    Destroy(displayed[i]);
                    displayed.Remove(i);
                }

                // Dodaj prefab przedmiotu
                ui = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);
                displayed[i] = ui;

                // Ustaw ilość
                var text = ui.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = slot.amount.ToString();
            }
            else
            {
                // Slot pusty
                if (displayed.ContainsKey(i))
                {
                    Destroy(displayed[i]);
                    displayed.Remove(i);
                }

                // Dodaj pusty slot
                ui = Instantiate(emptySlotPrefab, Vector3.zero, Quaternion.identity, transform);
                displayed[i] = ui;

                var text = ui.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = ""; // brak ilości
            }

            // Pozycja i podświetlenie
            ui.GetComponent<RectTransform>().localPosition = new Vector3(X_START + i * X_SPACE, 0, 0);
            ui.transform.localScale = (i == selectedIndex) ? Vector3.one * 1.12f : Vector3.one;
        }




    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;
    }
}
