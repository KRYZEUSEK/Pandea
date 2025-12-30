using UnityEngine;
using System.Collections;
// Dodajemy to, aby obs³u¿yæ NavMeshAgent (widzê go na screenie)
using UnityEngine.AI;

public class FreezeTrap : MonoBehaviour
{
    [Tooltip("Czas w sekundach, na ile gracz zostanie unieruchomiony")]
    public float freezeDuration = 3.0f;

    private void OnTriggerEnter(Collider other)
    {
        // Sprawdzamy po tagu "Player"
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FreezeAndDestroy(other.gameObject));
        }
    }

    IEnumerator FreezeAndDestroy(GameObject player)
    {
        var movementScript = player.GetComponent<PlayerControllerClick>();

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        // 1. Wy³¹czamy sterowanie
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // Dodatkowe zabezpieczenie: Zatrzymujemy agenta, ¿eby postaæ nie "doœlizgnê³a siê" do celu
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // Zerujemy prêdkoœæ natychmiast
        }

        // Ukrywamy pu³apkê
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // 2. Czekamy 3 sekundy
        yield return new WaitForSeconds(freezeDuration);

        // 3. Przywracamy sterowanie
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        if (agent != null)
        {
            agent.isStopped = false;
        }

        // 4. Niszczymy pu³apkê
        Destroy(gameObject);
    }
}