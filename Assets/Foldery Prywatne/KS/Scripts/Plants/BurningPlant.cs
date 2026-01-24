using System.Collections;
using UnityEngine;

public class BurningPlant : BasePlant
{
    [Header("Atak - Fala Ognia")]
    public GameObject projectilePrefab; // Prefab FireProjectile
    public Transform spawnPoint;        // Punkt tu¿ przy ziemi (¿eby fala sz³a do³em)
    public float shootInterval = 3.0f;  // Co ile sekund fala
    public int projectilesAmount = 12;  // Ile pocisków w jednym okrêgu (im wiêcej, tym gêstsza fala)

    private Coroutine shootingCoroutine;
    private Transform targetPlayer;

    // --- Implementacja BasePlant (Bez zmian w logice detekcji) ---

    protected override void OnPlayerEnter(GameObject player)
    {
        targetPlayer = player.transform;
        if (shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootRoutine());
        }
    }

    protected override void OnPlayerExit(GameObject player)
    {
        targetPlayer = null;
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    // --- Logika Fali Uderzeniowej ---

    IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(0.5f); // OpóŸnienie na start

        while (targetPlayer != null)
        {
            ShootCircularWave();
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootCircularWave()
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        // Obliczamy k¹t miêdzy ka¿dym pociskiem (360 stopni / iloœæ pocisków)
        float angleStep = 360f / projectilesAmount;

        for (int i = 0; i < projectilesAmount; i++)
        {
            // 1. Obliczamy rotacjê dla danego pocisku
            float currentAngle = i * angleStep;

            // Tworzymy rotacjê wokó³ osi Y (p³asko po ziemi)
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

            // 2. Tworzymy pocisk w punkcie spawnu z wyliczon¹ rotacj¹
            GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, rotation);

            // Opcjonalnie: Ignorujemy kolizjê miêdzy pociskiem a roœlin¹, ¿eby nie wybuch³ od razu
            Collider plantCollider = GetComponent<Collider>();
            Collider projCollider = proj.GetComponent<Collider>();
            if (plantCollider != null && projCollider != null)
            {
                Physics.IgnoreCollision(plantCollider, projCollider);
            }
        }
    }
}