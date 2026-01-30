using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventoryObject inventory;

    [Header("Przedmioty na Start")]
    public List<ItemObject> itemsToAddOnStart; // Przeci¹gnij przedmioty w Inspektorze

    private void Start()
    {
        // Dodawanie przedmiotów z listy na pocz¹tku gry
        foreach (var item in itemsToAddOnStart)
        {
            if (item != null)
            {
                inventory.AddItem(item, 1);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<Item>();
        if (item)
        {
            bool added = inventory.AddItem(item.item, 1);
            if (added)
            {
                Destroy(other.gameObject); // usuñ tylko jeœli dodano
            }
            else
            {
                Debug.Log("Nie mo¿na podnieœæ przedmiotu – ekwipunek pe³ny!");
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Wyczyszczenie ca³ej tablicy ekwipunku
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            inventory.Slots[i] = null;
        }
    }
}


