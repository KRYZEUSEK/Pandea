using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementAlterPlant : BasePlant
{
    [Header("Wartoœæ zmiany Prêdkoœci Ruchu")]
    [Tooltip("O ile zwiêkszyæ prêdkoœæ (np. 1.5)")]
    public float alterMovementValue = 1.5f;

    [Header("Czas trwania efektu")]
    public float duration = 3f;

    // Nadpisujemy metodê z BasePlant
    protected override void OnPlayerEnter(GameObject player)
    {
        // Próbujemy pobraæ NavMeshAgenta z obiektu, który wszed³ w roœlinê
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        // Sprawdzamy, czy gracz faktycznie ma NavMeshAgenta
        if (agent != null)
        {
            // Uruchamiamy procedurê zmiany prêdkoœci
            StartCoroutine(RestoreMovement(agent));

            // --- DEAKTYWACJA WIZUALNA I FIZYCZNA ROŒLINY ---

            // Szukamy WSZYSTKICH Rendererów, aby roœlina zniknê³a
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }

            // Wy³¹czamy wszystkie Collidery, ¿eby nie aktywowaæ tej samej roœliny ponownie
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.enabled = false;
            }
        }
    }

    IEnumerator RestoreMovement(NavMeshAgent agent)
    {
        // 1. Zwiêkszamy prêdkoœæ o zadan¹ wartoœæ
        agent.speed += alterMovementValue;

        // 2. Czekamy przez czas okreœlony w zmiennej duration
        yield return new WaitForSeconds(duration);

        // 3. Sprawdzamy, czy agent nadal istnieje (zabezpieczenie przed b³êdami NullReference)
        if (agent != null)
        {
            // KLUCZOWA POPRAWKA:
            // Odejmujemy dok³adnie tyle, ile dodaliœmy. 
            // Dziêki temu nawet jeœli gracz podniós³ 5 roœlin, ka¿da z nich 
            // "odda" swoj¹ porcjê prêdkoœci po up³ywie swojego czasu.
            agent.speed -= alterMovementValue;
        }

        // 4. Ca³kowicie usuwamy/dezaktywujemy obiekt roœliny z hierarchii
        // Jeœli korzystasz z Object Poolingu, SetActive(false) jest OK.
        // Jeœli to obiekty jednorazowe, mo¿esz u¿yæ Destroy(gameObject).
        Destroy(gameObject);
    }
}