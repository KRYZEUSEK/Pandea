using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteraction1 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handSocket;       // miejsce, gdzie item jest trzymany
    [SerializeField] private Collider playerCollider;    // główny collider gracza
    [SerializeField] private GameObject pickupTextUI;    // UI podpowiedzi "Naciśnij E"

    [Header("Inventory")]
    public InventoryObject inventory;                     // Twój InventoryObject
    [SerializeField] private HotbarSelector hotbarSelector; // Hotbar do przełączania

    private GameObject heldTool = null;                  // aktualnie trzymany item w ręce


    void Start()
    {
        if (pickupTextUI != null) pickupTextUI.SetActive(false);
        if (playerCollider == null) playerCollider = GetComponentInParent<Collider>();
    }

    void Update()
    {
        if (heldTool != null && heldTool.gameObject == null)
            heldTool = null;


        // Podnoszenie / upuszczanie itemu
        if (Input.GetKeyDown(KeyCode.E))
        {
                DropToolFromHand();
        }

        // Aktualizacja przedmiotu w ręce z hotbara
        UpdateHeldToolFromHotbar();
    }

    // ----------------------------------------
    // ---- HOTBAR / TRZYMANIE ITEMU ----
    // ----------------------------------------

    private void UpdateHeldToolFromHotbar()
    {
        if (hotbarSelector == null || 5 == 0)
        {
            ClearHeldTool();
            return;
        }

        int index = hotbarSelector.CurrentIndex;
        if (index >= 5)
        {
            ClearHeldTool();
            return;
        }

        var slot = inventory.Slots[index];

        // Jeśli slot pusty → usuń trzymany przedmiot
        if (slot == null || slot.item == null)
        {
            ClearHeldTool();
            return;
        }

        // Jeśli już trzymamy ten item, nic nie rób
        if (heldTool != null && heldTool.name == slot.item.name + "_Hand")
            return;

        // Usuń poprzedni
        ClearHeldTool();

        // Użyj worldPrefab zamiast prefab
        GameObject prefabToUse = slot.item.worldPrefab != null ? slot.item.worldPrefab : slot.item.prefab;
        if (prefabToUse == null) return;

        heldTool = Instantiate(prefabToUse, handSocket);
        heldTool.name = slot.item.name + "_Hand";
        heldTool.transform.localPosition = Vector3.zero;
        heldTool.transform.localScale = slot.item.handScale;
        heldTool.transform.localRotation = Quaternion.Euler(slot.item.handRotation);

        // Wyłącz kolizje i fizykę
        foreach (var c in heldTool.GetComponentsInChildren<Collider>())
            c.enabled = false;

        if (heldTool.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        if (heldTool.TryGetComponent<Collider>(out var col))
            Physics.IgnoreCollision(playerCollider, col, true);
    }

    private void ClearHeldTool()
    {
        if (heldTool != null)
        {
            Destroy(heldTool);
            heldTool = null;
        }
    }


    // ----------------------------------------
    // ---- UPUSZCZANIE ITEMU Z RĘKI ----
    // ----------------------------------------

    private void DropToolFromHand()
    {
        int index = hotbarSelector.CurrentIndex;
        if (index >= 5) return;

        var slot = inventory.Slots[index];
        if (slot == null || slot.amount <= 0) return;

        // Usuń z ekwipunku
        inventory.RemoveItem(slot.item, 1);

        // Usuń prefab z ręki
        ClearHeldTool();

        // Spawn w świecie
        Vector3 spawnPos = transform.position + Vector3.up * 2f + transform.forward * 4f;
        GameObject droppedObj = Instantiate(slot.item.worldPrefab, spawnPos, Quaternion.identity);

        if (!droppedObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb = droppedObj.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(transform.forward * 3f, ForceMode.Impulse);

        if (!droppedObj.TryGetComponent<Collider>(out Collider col))
            droppedObj.AddComponent<BoxCollider>();

        if (!droppedObj.TryGetComponent<ItemPickup>(out ItemPickup pickup))
        {
            pickup = droppedObj.AddComponent<ItemPickup>();
            pickup.itemData = slot.item;
        }

        Debug.Log("Wyrzucono: " + slot.item.name);
    }



}
