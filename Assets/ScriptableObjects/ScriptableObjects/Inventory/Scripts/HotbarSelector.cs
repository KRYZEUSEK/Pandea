using System;
using UnityEngine;

public class HotbarSelector : MonoBehaviour
{
    public InventoryObject inventory;
    public DisplayHotbar displayHotbar;
    public GameObject zielnikUI;
    public int CurrentIndex { get; private set; } = 0;
    public event Action<int> OnSelectedIndexChanged;

    void Awake()
    {
        // --- ZMIANY: Automatyczne pobieranie referencji dla Prefabu ---

        // 1. Szukamy ekwipunku na scenie
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryObject>(FindObjectsInactive.Include);

        // 2. Szukamy skryptu DisplayHotbar na scenie (zapewne jest przypiêty do jakiegoœ panelu UI)
        if (displayHotbar == null)
            displayHotbar = FindFirstObjectByType<DisplayHotbar>(FindObjectsInactive.Include);

        // 3. Szukamy panelu Zielnika (szukamy po wszystkich Canvasach, ¿eby znaleŸæ nawet wy³¹czony obiekt)
        if (zielnikUI == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                Transform[] children = c.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    // Wzoruj¹c siê na Twoim screenie z poprzedniego pytania, obiekt nazywa³ siê "Herbarium"
                    if (child.name == "Herbarium")
                    {
                        zielnikUI = child.gameObject;
                        break;
                    }
                }
                if (zielnikUI != null) break;
            }
        }
        // --------------------------------------------------------------
    }

    void Start()
    {
        SelectSlot(0);
    }

    void Update()
    {
        if (zielnikUI != null && zielnikUI.activeInHierarchy) return;
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
        if (inventory != null && inventory.Slots != null && CurrentIndex < inventory.Slots.Length)
        {
            var slot = inventory.Slots[CurrentIndex];
            if (slot == null || slot.item == null)
            {
                Debug.Log($"Wybrano pusty slot {CurrentIndex}");
            }
            else
            {
                Debug.Log($"Wybrano slot {CurrentIndex}: {slot.item.name}");
            }
        }

        // Podœwietlenie w UI (zawsze dzia³a, nawet dla pustych slotów)
        if (displayHotbar != null)
            displayHotbar.SetSelectedIndex(CurrentIndex);

        OnSelectedIndexChanged?.Invoke(CurrentIndex);
    }

    public bool IsWrenchEquipped()
    {
        return IsItemEquipped("wrench");
    }

    // NOWE: ogólna metoda + skrót dla siekiery
    public bool IsAxeEquipped()
    {
        return IsItemEquipped("axe");
    }

    public bool IsItemEquipped(string id)
    {
        if (inventory == null || inventory.Slots == null) return false;
        if (CurrentIndex < 0 || CurrentIndex >= inventory.Slots.Length) return false;

        var slot = inventory.Slots[CurrentIndex];
        if (slot == null || slot.item == null) return false;

        // U Ciebie: public string id w ItemObject
        return slot.item.id == id;
    }
}