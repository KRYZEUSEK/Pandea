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

    public bool HasItems(IEnumerable<BuildCost> costs)
    {
        foreach (var cost in costs)
        {
            if (cost == null || cost.item == null || cost.amount <= 0)
                return false;

            int total = 0;
            for (int i = 0; i < Slots.Length; i++)
            {
                var s = Slots[i];
                if (s != null && s.item != null && s.item.id == cost.item.id)
                {
                    total += s.amount;
                }
            }

            if (total < cost.amount)
                return false; // brakuje któregoś kosztu
        }
        return true;
    }

    public bool ConsumeItems(IEnumerable<BuildCost> costs)
    {
        // Najpierw walidacja, żeby nie częściowo zużyć.
        if (!HasItems(costs))
            return false;

        // Zużycie — iterujemy po kosztach i odejmujemy z odpowiednich slotów.
        foreach (var cost in costs)
        {
            int leftToConsume = cost.amount;

            for (int i = 0; i < Slots.Length && leftToConsume > 0; i++)
            {
                var s = Slots[i];
                if (s != null && s.item != null && s.item.id == cost.item.id && s.amount > 0)
                {
                    int take = Mathf.Min(s.amount, leftToConsume);
                    s.amount -= take;
                    leftToConsume -= take;

                    // Opcjonalnie wyczyść slot jeśli amount spadł do 0
                    if (s.amount <= 0)
                    {
                        s.item = null;
                        s.amount = 0;
                    }
                }
            }

            // Po tej pętli leftToConsume powinno być 0 — bo HasItems to gwarantował
        }
        OnInventoryChanged?.Invoke();
        return true;
    }
    public void SwapSlots(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || fromIndex < 0 || toIndex < 0 || fromIndex >= Slots.Length || toIndex >= Slots.Length)
            return;

        var temp = Slots[fromIndex];
        Slots[fromIndex] = Slots[toIndex];
        Slots[toIndex] = temp;

        OnInventoryChanged?.Invoke();
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
