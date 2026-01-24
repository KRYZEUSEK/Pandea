using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StalkerEnemy : BaseEnemy
{
    [Header("Specyfika Stalkera")]
    [Tooltip("Dystans, jaki przeciwnik stara siê utrzymaæ od gracza.")]
    public float desiredDistance = 7.0f;

    [Header("Logika Szar¿y")]
    [Tooltip("Szansa (0.0 do 1.0) na szar¿ê co 10 sekund.")]
    [Range(0f, 1f)]
    public float chargeChance = 0.5f;

    [Tooltip("Prêdkoœæ poruszania siê podczas szar¿y.")]
    public float chargeSpeed = 10f;

    [Tooltip("Maksymalny czas trwania szar¿y, jeœli nie trafi gracza (w sekundach).")]
    public float chargeDuration = 2.0f;

    [Tooltip("Jak daleko 'za' gracza ma celowaæ przeciwnik podczas szar¿y.")]
    public float chargeOvershootDistance = 5f;

    [Tooltip("Ile punktowo ma czasu zabraæ szar¿a po trafieniu nas.")]
    public float chargedmg = 10f;

    // --- NOWA ZMIENNA (PRZEWIDYWANIE) ---
    [Header("Przewidywanie Celu")]
    [Tooltip("Jak daleko w przysz³oœæ (w sekundach) przeciwnik ma przewidywaæ ruch gracza.")]
    public float predictionTime = 0.5f;

    // --- Zmienne Wewnêtrzne ---
    private float chargeTimer = 10f;
    private const float CHARGE_INTERVAL = 10f;
    private bool isCharging = false;
    private float originalSpeed;
    private float currentChargeTimer;
    private bool hasHitPlayerThisCharge = false;
    private Vector3 chargeTargetLocation;

    // --- NOWA ZMIENNA (AGENT GRACZA) ---
    private NavMeshAgent playerAgent; // Potrzebne do odczytania prêdkoœci gracza

    /// <summary>
    /// Nadpisujemy Awake(), aby pobraæ nie tylko agenta wroga, ale i agenta gracza.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // To pobiera agenta WROGA i transform gracza
        if (agent != null)
        {
            originalSpeed = agent.speed;
        }

        // --- NOWA LOGIKA ---
        // 'player' (Transform) jest pobierany w BaseEnemy.Awake()
        if (player != null)
        {
            // Na podstawie transformu gracza, pobieramy jego komponent NavMeshAgent
            playerAgent = player.GetComponent<NavMeshAgent>();
            if (playerAgent == null)
            {
                Debug.LogWarning("StalkerEnemy: Nie znaleziono NavMeshAgent na graczu. Przewidywanie nie bêdzie dzia³aæ.");
            }
        }
    }

    protected override void UpdateEnemyBehavior()
    {
        // 1. Logika Timera Szar¿y (inicjacja)
        if (!isCharging)
        {
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0f)
            {
                chargeTimer = CHARGE_INTERVAL;
                if (Random.value < chargeChance)
                {
                    StartCharge(); // Uruchom szar¿ê
                }
            }
        }

        // 2. Logika Ruchu (Szar¿a)
        if (isCharging)
        {
            // Przeciwnik nadal biegnie do ZAPISANEJ lokalizacji
            agent.SetDestination(chargeTargetLocation);

            currentChargeTimer -= Time.deltaTime;

            // SprawdŸ, czy czas siê skoñczy³ lub czy dotar³ do celu
            // (Dodajemy sprawdzenie dotarcia do celu, aby nie sta³ g³upio)
            float distanceToTarget = Vector3.Distance(transform.position, chargeTargetLocation);

            if (currentChargeTimer <= 0f || distanceToTarget < 1.0f)
            {
                if (currentChargeTimer <= 0f)
                    Debug.Log("Szar¿a przekroczy³a limit czasu (2s) i nie trafi³a.");
                else
                    Debug.Log("Szar¿a dotar³a do celu i nie trafi³a.");

                StopCharge(); // Przerwij szar¿ê
            }
        }
        else
        {
            // --- LOGIKA TRZYMANIA DYSTANSU (bez zmian) ---
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < desiredDistance - 0.5f)
            {
                Vector3 directionAway = (transform.position - player.position).normalized;
                Vector3 fleeDestination = transform.position + directionAway * 5f;

                if (NavMesh.SamplePosition(fleeDestination, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
            else
            {
                agent.SetDestination(player.position);
            }
        }
    }

    /// <summary>
    /// Rozpoczyna sekwencjê szar¿y.
    /// </summary>
    private void StartCharge()
    {
        isCharging = true;
        hasHitPlayerThisCharge = false;
        currentChargeTimer = chargeDuration;

        agent.speed = chargeSpeed;
        agent.stoppingDistance = 0f;

        // --- ZMODYFIKOWANA LOGIKA KIERUNKU (PRZEWIDYWANIE) ---

        Vector3 playerCurrentPos = player.position;
        Vector3 predictedPlayerPos = playerCurrentPos; // Domyœlnie, jeœli nie ma agenta

        // SprawdŸ, czy mamy agenta gracza, aby odczytaæ jego prêdkoœæ
        if (playerAgent != null)
        {
            Vector3 playerVelocity = playerAgent.velocity;
            predictedPlayerPos = playerCurrentPos + (playerVelocity * predictionTime);
        }

        // Oblicz kierunek od nas do PRZEWIDYWANej pozycji gracza
        Vector3 directionToPredictedPos = (predictedPlayerPos - transform.position).normalized;

        // Oblicz i ZAPISZ docelowy punkt, który znajduje siê 'za' PRZEWIDYWAN¥ pozycj¹
        chargeTargetLocation = predictedPlayerPos + (directionToPredictedPos * chargeOvershootDistance);

        Debug.Log("Przeciwnik szar¿uje na PRZEWIDZIANY cel!");
    }

    /// <summary>
    /// Koñczy sekwencjê szar¿y (po trafieniu lub po up³ywie czasu).
    /// </summary>
    private void StopCharge()
    {
        isCharging = false;
        agent.speed = originalSpeed;
        agent.stoppingDistance = desiredDistance;
    }

    /// <summary>
    /// Logika trafienia (bez zmian - nadal oparta na fizyce).
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isCharging)
            return;

        if (other.CompareTag("Player"))
        {
            if (hasHitPlayerThisCharge)
                return;

            Debug.Log("Gracz trafiony (KOLIZJA)! Odejmujê 10 jednostek czasu.");
            hasHitPlayerThisCharge = true;

            if (TimeManager.Instance != null)
                TimeManager.Instance.ModifyTime(-chargedmg);

            StopCharge();
        }
    }
}