using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementAlterPlant : BasePlant
{
    [Header("Wartoœæ zmiany Prêdkoœci Ruchu")]
    public float alterMovementValue = 1.5f; // O ile zwiêkszyæ prêdkoœæ

    [Header("Po jakim czasie wartoœæ ruchu ma wróciæ do podstawowej?")]
    public float duration = 3f; // Czas trwania efektu w sekundach

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
            this.gameObject.SetActive(false); // Dezaktywujemy roœlinê po u¿yciu
        }
    }

    IEnumerator RestoreMovement(NavMeshAgent agent)
    {
        // 1. Zapamiêtujemy aktualn¹ (oryginaln¹) prêdkoœæ
        float originalSpeed = agent.speed;

        // 2. Modyfikujemy prêdkoœæ (dodajemy wartoœæ)
        agent.speed += alterMovementValue;

        // 3. Czekamy 3 sekundy (zgodnie z proœb¹)
        yield return new WaitForSeconds(duration);

        // 4. Sprawdzamy czy agent nadal istnieje (zabezpieczenie, gdyby gracz zgin¹³/znikn¹³ w miêdzyczasie)
        if (agent != null)
        {
            // Przywracamy zapamiêtan¹ oryginaln¹ prêdkoœæ
            agent.speed = originalSpeed;
        }


    }
}
