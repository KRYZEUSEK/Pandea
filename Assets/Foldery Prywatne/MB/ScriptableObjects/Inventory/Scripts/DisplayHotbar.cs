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
    private int selectedIndex = 0; // Zmień na 0, żeby od początku coś było wybrane

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
        // WYCZYŚĆ RĘCZNIE DODANE OBIEKTY W EDYTORZE (jeśli jakieś są podpięte pod ten transform)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Wywołaj tylko raz
        RefreshHotbar();
    }

    public void RefreshHotbar()
    {
        // Czyścimy słownik i niszczymy stare obiekty
        foreach (var item in displayed.Values)
        {
            Destroy(item);
        }
        displayed.Clear();

        for (int i = 0; i < 5; i++)
        {
            var slot = Inventory.Slots[i];
            GameObject ui;

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
            RectTransform rt = ui.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(X_START + i * X_SPACE, 0, 0);

            // Ustawienie skali na starcie
            ui.transform.localScale = (i == selectedIndex) ? Vector3.one * 1.12f : Vector3.one;

            displayed[i] = ui;
        }
    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;

        // Odświeżamy skalę dla wszystkich wyświetlanych slotów
        foreach (var kvp in displayed)
        {
            if (kvp.Value != null)
            {
                kvp.Value.transform.localScale = (kvp.Key == selectedIndex) ? Vector3.one * 1.12f : Vector3.one;
            }
        }
    }
}