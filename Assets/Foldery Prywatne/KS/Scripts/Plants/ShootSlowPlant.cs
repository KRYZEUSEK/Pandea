using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSlowPlant : BasePlant
{
    [Header("Atak - Spowalniaj¹cy Strza³")]
    public GameObject projectilePrefab;
    public Transform spawnPoint;
    public float shootInterval = 3.0f;  // Przerwa miêdzy ca³¹ seri¹
    public int projectilesAmount = 12;  // Iloœæ strza³ów w jednej serii
    public float delayBetweenShots = 0.1f; // Przerwa miêdzy pojedynczymi kulkami w serii

    private Coroutine shootingCoroutine;
    private Transform targetPlayer;


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
        if (targetPlayer != null && player.transform == targetPlayer)
        {
            targetPlayer = null;
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
        }
    }

    IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (targetPlayer != null)
        {
            // Zmieniamy wywo³anie na StartCoroutine, bo teraz strzelanie te¿ zajmuje czas
            yield return StartCoroutine(ShootSeries());

            // Czekamy przed kolejn¹ ca³¹ seri¹
            yield return new WaitForSeconds(shootInterval);
        }
    }

    IEnumerator ShootSeries()
    {
        for (int i = 0; i < projectilesAmount; i++)
        {
            if (projectilePrefab == null || spawnPoint == null) yield break;

            // Losujemy kierunek
            float randomAngle = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);

            // Tworzymy pocisk
            GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, rotation);

            // Ignorowanie kolizji
            Collider projCollider = proj.GetComponent<Collider>();
            if (plantCollider != null && projCollider != null)
            {
                Physics.IgnoreCollision(plantCollider, projCollider);
            }

            // --- KLUCZOWA ZMIANA ---
            // Czekamy krótk¹ chwilê przed wystrzeleniem kolejnej kulki w serii
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }
}