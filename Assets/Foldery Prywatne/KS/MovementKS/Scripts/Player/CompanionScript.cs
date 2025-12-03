using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Wymusza obecnoœæ komponentu NavMeshAgent na obiekcie
[RequireComponent(typeof(NavMeshAgent))]
public class CompanionScript : MonoBehaviour
{
    [Header("Ustawienia Dystansu")]
    [Tooltip("Po¿¹dana minimalna odleg³oœæ od gracza.")]
    public float desiredDistance = 7.0f;

    [Tooltip("Prêdkoœæ, z jak¹ kompan siê porusza.")]
    public float moveSpeed = 4.0f;

    [Tooltip("Bufor, w którym kompan nie podejmuje akcji, aby zapobiec drganiom.")]
    public float distanceBuffer = 0.5f;

    [Header("Urozmaicenie Ruchu")]
    [Tooltip("Szybkoœæ, z jak¹ kompan reaguje na zmianê kierunku.")]
    public float rotationSpeed = 10f;

    [Tooltip("Maksymalna odleg³oœæ losowego offsetu celu.")]
    public float maxOffset = 1.5f;

    [Tooltip("Co ile sekund zmieniaæ losowy offset celu.")]
    public float offsetChangeInterval = 2.0f;

    // Odniesienia
    private Transform player;
    private NavMeshAgent agent;

    // Zmienne wewnêtrzne dla logiki b³¹dzenia
    private float offsetTimer;
    private Vector3 randomOffset;

    void Awake()
    {
        // Pobierz komponent NavMeshAgent, ustaw prêdkoœæ i WY£¥CZ automatyczny obrót
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.updateRotation = false; // <-- Kluczowe dla p³ynnego obrotu

        // ZnajdŸ gracza po tagu "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Nie znaleziono obiektu gracza! Upewnij siê, ¿e gracz ma tag 'Player'.");
        }
    }

    void Update()
    {
        // Upewnij siê, ¿e mamy gracza i agent jest aktywny
        if (player == null || agent == null || !agent.enabled) return;

        MaintainDistance();
        ApplyRotation(); // P³ynne obracanie w kierunku ruchu
    }

    /// <summary>
    /// G³ówna logika utrzymywania po¿¹danej odleg³oœci od gracza (bez uciekania).
    /// </summary>
    void MaintainDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Ustaw agenta, aby ZATRZYMA£ siê w po¿¹danej odleg³oœci
        agent.stoppingDistance = desiredDistance;

        // --- Timer i Losowanie Offsetu ---
        offsetTimer -= Time.deltaTime;
        if (offsetTimer <= 0f)
        {
            // Generuj nowy, losowy offset (w promieniu maxOffset)
            randomOffset = Random.insideUnitSphere * maxOffset;
            randomOffset.y = 0; // Upewnij siê, ¿e pozostaje na ziemi
            offsetTimer = offsetChangeInterval;
        }

        // --- Logika: Jesteœmy ZA DALEKO ---
        // Dystans jest wiêkszy ni¿ po¿¹dana odleg³oœæ plus bufor
        if (distanceToPlayer > desiredDistance + distanceBuffer)
        {
            // Cel to pozycja gracza + losowy offset (meandrowanie)
            Vector3 targetPosition = player.position + randomOffset;

            // U¿yj NavMesh.SamplePosition, aby upewniæ siê, ¿e cel jest na siatce NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        // --- Logika: Jesteœmy ZA BLISKO lub w SAM RAZ ---
        // Nic nie rób. Agent jest bezczynny, dopóki gracz siê nie oddali.
    }

    /// <summary>
    /// P³ynnie obraca kompana w kierunku jego aktualnego ruchu (poniewa¿ agent.updateRotation jest wy³¹czone).
    /// </summary>
    void ApplyRotation()
    {
        // SprawdŸ, czy agent siê porusza i czy ma œcie¿kê
        if (agent.velocity.sqrMagnitude > 0.1f && agent.hasPath)
        {
            // Oblicz kierunek ruchu (tylko na p³aszczyŸnie XZ)
            Vector3 direction = agent.velocity.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // P³ynny obrót za pomoc¹ Quaternion.Slerp
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}