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

        // Probujemy pobrac NavMeshAgenta z obiektu, ktory wszedl w rosline
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        // Sprawdzamy, czy gracz faktycznie ma NavMeshAgenta
        if (agent != null)
        {
            hasBeenActivated = true;

            // Uruchamiamy procedure zmiany predkosci
            StartCoroutine(RestoreMovement(agent));

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
        }
    }

    IEnumerator RestoreMovement(NavMeshAgent agent)
    {
        // 1. Zwiekszamy predkosc o zadana wartosc
        agent.speed += alterMovementValue;

        // 2. Czekamy przez czas okreslony w zmiennej duration
        yield return new WaitForSeconds(duration);

        // 3. Sprawdzamy, czy agent nadal istnieje (zabezpieczenie przed bledami NullReference)
        if (agent != null)
        {
            // KLUCZOWA POPRAWKA:
            // Odejmujemy dokladnie tyle, ile dodalismy. 
            // Dzieki temu nawet jesli gracz podniosl 5 roslin, kazda z nich 
            // "odda" swoja porcje predkosci po uplywie swojego czasu.
            agent.speed -= alterMovementValue;
        }

        // 4. Calkowicie usuwamy/dezaktywujemy obiekt rosliny z hierarchii
        // Jesli korzystasz z Object Poolingu, SetActive(false) is OK.
        // Jesli to obiekty jednorazowe, mozesz uzyc Destroy(gameObject).
        Destroy(gameObject);
    }
}
