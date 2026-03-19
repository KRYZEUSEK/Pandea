using UnityEngine;
using System.Collections.Generic;

public class PickupFlower : MonoBehaviour
{
    [Header("Ustawienia Interakcji")]
    [SerializeField] private KeyCode actionKey = KeyCode.O; // Klawisz do interakcji (domyœlnie 'O')
    [SerializeField] private float range = 15f; // zasiêg interakcji

    [Header("Referencje")]
    [SerializeField] private HotbarSelector hotbarSelector;
    [SerializeField] private Camera playerCam; 

    [Header("Loot Settings")]
    [SerializeField] private GameObject plantItemPrefab; // Tutaj wstaw prefab 'Plant' z folderu Items3D narazie tylko dla jednego typu roœliny, ale mo¿na rozbudowaæ o ró¿ne prefaby dla ró¿nych roœlin

    private void Awake()
    {
        if (hotbarSelector == null) hotbarSelector = GetComponent<HotbarSelector>();
        if (playerCam == null) playerCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(actionKey))
        {
            if (hotbarSelector != null && hotbarSelector.IsAxeEquipped()) // Czy wybrana jest siekiera przedmiot do niszczenia roslin, mo¿na to rozbudowaæ o inne narzêdzia i roœliny w przysz³oœci
            {
                TryTargetPlant();
            }
        }
    }

    private void TryTargetPlant()
    {
        if (playerCam == null) return;

        Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        if (Physics.Raycast(ray, out hit, range, Physics.AllLayers, QueryTriggerInteraction.Collide)) // sprawdzamy czy trafiliœmy w jakiœ collider, uwzglêdniaj¹c trigger (roœliny s¹ triggerami)
        {
            BasePlant plant = hit.collider.GetComponentInParent<BasePlant>();

            if (plant != null)
            {
                CollectPlant(plant);
            }
            else
            {
                Debug.Log("<color=orange>Pud³o!</color> Trafi³eœ w: " + hit.collider.gameObject.name);
            }
        }
    }

    private void CollectPlant(BasePlant plant)
    {
        Debug.Log("<color=green>CEL OSI¥GNIÊTY:</color> Œciêto " + plant.gameObject.name);

        // --- NOWA LOGIKA WYPADANIA PRZEDMIOTU ---
        if (plantItemPrefab != null)
        {
            // Spawnujemy przedmiot w miejscu roœliny, lekko nad ziemi¹
            Vector3 spawnPos = plant.transform.position + Vector3.up * 0.5f;
            GameObject droppedItem = Instantiate(plantItemPrefab, spawnPos, Quaternion.identity);

            // Dodajemy fizykê, ¿eby przedmiot upad³ na ziemiê
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb == null) rb = droppedItem.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;

            // Opcjonalnie: lekki "podskok" przedmiotu przy wypadaniu
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);

            // Upewniamy siê, ¿e przedmiot ma skrypt do podnoszenia (ItemPickup)
            if (!droppedItem.TryGetComponent<ItemPickup>(out ItemPickup pickup))
            {
                pickup = droppedItem.AddComponent<ItemPickup>();
                // Tutaj skrypt sam powinien pobraæ dane, jeœli masz to ustawione w prefabie
            }
        }

        // Niszczymy roœlinê
        Destroy(plant.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (playerCam != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerCam.transform.position, playerCam.transform.forward * range);
        }
    }
}