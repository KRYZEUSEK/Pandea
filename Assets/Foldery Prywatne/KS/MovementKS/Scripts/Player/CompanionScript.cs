using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Wymusza obecność komponentu NavMeshAgent i AudioSource
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))] 
public class CompanionScript : MonoBehaviour
{
    [Header("Ustawienia Dystansu (Oryginalne)")]
    [Tooltip("Pożądana minimalna odległość od gracza.")]
    public float desiredDistance = 7.0f;

    [Tooltip("Prędkość, z jaką kompan się porusza.")]
    public float moveSpeed = 4.0f;

    [Tooltip("Bufor, w którym kompan nie podejmuje akcji.")]
    public float distanceBuffer = 0.5f;

    [Header("Urozmaicenie Ruchu (Oryginalne)")]
    [Tooltip("Szybkość, z jaką kompan reaguje na zmianę kierunku.")]
    public float rotationSpeed = 10f;

    [Tooltip("Maksymalna odległość losowego offsetu celu.")]
    public float maxOffset = 1.5f;

    [Tooltip("Co ile sekund zmieniać losowy offset celu.")]
    public float offsetChangeInterval = 2.0f;

    // --- NOWE: Zmienne do systemu wody i tlenu ---
    [Header("--- NOWE: System Wody ---")]
    public ParticleSystem waterParticles; // Przypisz w inspektorze
    public float maxWater = 100f;
    public float waterConsumptionRate = 3f; // Ile wody zużywa na sekundę
    public float waterRecoveryRate = 10f; // Ile wody regeneruje się w bańce
    [SerializeField] private float currentWater; // Widoczne w inspektorze do podglądu

    [Header("--- NOWE: System Tlenu ---")]
    public TimeManager czas;    // Skrypt gracza (status)
    public AudioClip lowOxygenAlert;     // Dźwięk alarmu
    [Range(0, 100)]
    public float alertThreshold = 20f;   // Poziom alarmowy (20%)
    
    private AudioSource audioSource;
    private bool isAlerting = false;
    // ---------------------------------------------

    // Odniesienia
    private Transform player;
    private NavMeshAgent agent;

    // Zmienne wewnętrzne dla logiki błądzenia
    private float offsetTimer;
    private Vector3 randomOffset;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.updateRotation = false; 

        // Pobieramy AudioSource
        audioSource = GetComponent<AudioSource>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            
            // --- NOWE: Automatyczne znalezienie skryptu statusu gracza ---
            if (czas == null)
            {
                czas = playerObject.GetComponent<TimeManager>();
            }
        }
        else
        {
            Debug.LogError("Nie znaleziono obiektu gracza! Upewnij się, że gracz ma tag 'Player'.");
        }
    }

    // --- NOWE: Inicjalizacja wody ---
    void Start()
    {
        currentWater = maxWater;
        if (waterParticles != null)
        {
            var emission = waterParticles.emission;
            emission.enabled = false;
        }
    }

    void Update()
    {
        if (player == null || agent == null || !agent.enabled) return;

        MaintainDistance(); // Oryginalna logika ruchu

        // --- NOWE: Logika strzelania i tlenu ---
        bool isShooting = HandleShooting(); 
        CheckOxygen();

        // Jeśli strzelamy, obracamy się do kursora. Jeśli nie - obracamy się zgodnie z ruchem (oryginał)
        if (isShooting)
        {
            RotateTowardsCursor();
        }
        else
        {
            ApplyRotation();
        }
    }

    // --- NOWE: Obsługa strzelania wodą ---
    bool HandleShooting()
    {
        // Sprawdzamy czy wciśnięto LPM i czy jest woda
        if (Input.GetMouseButton(1) && currentWater > 0)
        {
            if (waterParticles != null)
            {
                var emission = waterParticles.emission;
                emission.enabled = true;
            }

            currentWater -= waterConsumptionRate * Time.deltaTime;
            currentWater = Mathf.Max(currentWater, 0); // Blokada na 0
            return true; // Zwracamy informację, że strzelamy
        }
        else
        {
            if (waterParticles != null)
            {
                var emission = waterParticles.emission;
                emission.enabled = false;
            }
            return false; // Nie strzelamy
        }
    }

    // --- NOWE: Obracanie kompana w stronę kursora ---
    void RotateTowardsCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Patrzymy w punkt uderzenia lasera myszki, ale zachowujemy wysokość kompana (żeby się nie przechylał)
            Vector3 targetPostition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Vector3 direction = (targetPostition - transform.position).normalized;
            
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * 2); // Szybszy obrót przy celowaniu
            }
        }
    }

    // --- NOWE: Sprawdzanie tlenu gracza ---
    void CheckOxygen()
    {
        if (czas == null || audioSource == null || lowOxygenAlert == null) return;

        if (czas.currentTime <= alertThreshold)
        {
            if (!isAlerting)
            {
                audioSource.PlayOneShot(lowOxygenAlert);
                isAlerting = true; 
            }
        }
        else
        {
            isAlerting = false; 
        }
    }

    // --- NOWE: Odnawianie wody ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AirZone"))
        {
            currentWater += waterRecoveryRate * Time.deltaTime;
            // Opcjonalnie dźwięk tankowania
            Debug.Log("Woda odnowiona!");
        }
    }
    
    void MaintainDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        agent.stoppingDistance = desiredDistance;

        offsetTimer -= Time.deltaTime;
        if (offsetTimer <= 0f)
        {
            randomOffset = Random.insideUnitSphere * maxOffset;
            randomOffset.y = 0; 
            offsetTimer = offsetChangeInterval;
        }

        if (distanceToPlayer > desiredDistance + distanceBuffer)
        {
            Vector3 targetPosition = player.position + randomOffset;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    void ApplyRotation()
    {
        if (agent.velocity.sqrMagnitude > 0.1f && agent.hasPath)
        {
            Vector3 direction = agent.velocity.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}