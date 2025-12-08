using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public InventorySlot[] Slots = new InventorySlot[20];
    public event Action OnInventoryChanged;


    public bool AddItem(ItemObject item, int amount)
    {
        // Szukaj istniejącego slotu
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] != null && Slots[i].item == item)
            {
                Slots[i].amount += amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Szukaj pustego miejsca
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == null || Slots[i].item == null)
            {
                Slots[i] = new InventorySlot(item, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.Log("Brak miejsca w ekwipunku!");
        return false; // nie udało się dodać
    }


    public void RemoveItem(ItemObject item, int amount)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] != null && Slots[i].item == item)
            {
                Slots[i].amount -= amount;
                if (Slots[i].amount <= 0)
                    Slots[i] = null; // Oznacz slot jako pusty
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

}






[System.Serializable]
public class InventorySlot
{
    public ItemObject item;
    public int amount;
    public InventorySlot(ItemObject _item, int _amount)
    {
        item = _item;
        amount = _amount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }

}
