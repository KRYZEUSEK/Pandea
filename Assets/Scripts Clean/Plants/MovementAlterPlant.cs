using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementAlterPlant : BasePlant
{
    [Header("Wartosc zmiany Predkosci Ruchu")]
    [Tooltip("O ile zwiekszyc predkosc (np. 1.5)")]
    public float alterMovementValue = 1.5f;

    [Header("Czas trwania efektu")]
    public float duration = 3f;

    private bool hasBeenActivated = false;

    // Nadpisujemy metode z BasePlant
    protected override void OnPlayerEnter(GameObject player)
    {
        if (hasBeenActivated) return;

        // Probujemy pobrac PlayerControllerClick1 lub PlayerControllerClick z obiektu gracza
        PlayerControllerClick1 controller1 = player.GetComponent<PlayerControllerClick1>();
        if (controller1 != null)
        {
            hasBeenActivated = true;

            // Dodajemy modyfikator predkosci z unikalnym ID
            string boostId = "PlantBoost_" + System.Guid.NewGuid().ToString();
            controller1.AddSpeedModifier(boostId, alterMovementValue, duration);

            DeactivateAndDestroy();
            return;
        }

        PlayerControllerClick controller = player.GetComponent<PlayerControllerClick>();
        if (controller != null)
        {
            hasBeenActivated = true;

            // Dodajemy modyfikator predkosci z unikalnym ID
            string boostId = "PlantBoost_" + System.Guid.NewGuid().ToString();
            controller.AddSpeedModifier(boostId, alterMovementValue, duration);

            DeactivateAndDestroy();
            return;
        }
    }

    private void DeactivateAndDestroy()
    {
        // --- DEAKTYWACJA WIZUALNA I FIZYCZNA ROSLINY ---

        // Szukamy WSZYSTKICH Rendererow, aby roslina zniknela
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        // Wylaczamy wszystkie Collidery, zeby nie aktywowac tej samej rosliny ponownie
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        // Niszczymy ten obiekt po uplywie czasu
        Destroy(gameObject, duration + 0.5f);
    }
}
