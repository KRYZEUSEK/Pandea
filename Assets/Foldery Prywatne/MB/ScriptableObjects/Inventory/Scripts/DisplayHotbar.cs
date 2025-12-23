using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayHotbar : MonoBehaviour
{
    public InventoryObject Inventory;
    public GameObject emptySlotPrefab;
    public int X_START = 0;
    public int X_SPACE = 100;

    private Dictionary<int, GameObject> displayed = new Dictionary<int, GameObject>();
    private int selectedIndex = -1;

    void OnEnable()
    {
        if (Inventory != null)
            Inventory.OnInventoryChanged += RefreshHotbar;
    }

    void OnDisable()
    {
        if (Inventory != null)
            Inventory.OnInventoryChanged -= RefreshHotbar;
    }

    void Start()
    {
        // Tworzymy sloty raz
        for (int i = 0; i < 5; i++)
        {
            var slot = Inventory.Slots[i];
            GameObject prefab = (slot != null && slot.item != null) ? slot.item.prefab : emptySlotPrefab;
            GameObject ui = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            displayed[i] = ui;

            ui.GetComponent<RectTransform>().localPosition = new Vector3(X_START + i * X_SPACE, 0, 0);
            ui.transform.localScale = (i == selectedIndex) ? Vector3.one * 1.12f : Vector3.one;
        }

        RefreshHotbar();
    }

    public void RefreshHotbar()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = Inventory.Slots[i];
            GameObject ui;

            // Usuń stary prefab, jeśli istnieje
            if (displayed.ContainsKey(i))
            {
                Destroy(displayed[i]);
                displayed.Remove(i);
            }

            // Wstaw nowy slot (item lub pusty)
            if (slot != null && slot.item != null)
            {
                ui = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);
                var text = ui.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = slot.amount.ToString();
            }
            else
            {
                ui = Instantiate(emptySlotPrefab, Vector3.zero, Quaternion.identity, transform);
                var text = ui.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = "";
            }

            // Pozycja i podświetlenie
            ui.GetComponent<RectTransform>().localPosition = new Vector3(X_START + i * X_SPACE, 0, 0);
            ui.transform.localScale = (i == selectedIndex) ? Vector3.one * 1.12f : Vector3.one;

            displayed[i] = ui;
        }
    }


    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;

        // Odśwież tylko podświetlenie
        foreach (var kvp in displayed)
        {
            kvp.Value.transform.localScale = (kvp.Key == selectedIndex) ? Vector3.one * 1.12f : Vector3.one;
        }
    }
}
