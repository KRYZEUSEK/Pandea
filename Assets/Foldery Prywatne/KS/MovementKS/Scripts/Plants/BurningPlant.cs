using System.Collections;
using UnityEngine;

public class BurningPlant : BasePlant
{
    [Header("Atak")]
    public GameObject projectilePrefab; // Tu przypisz prefab z FireProjectile
    public Transform spawnPoint;        // Sk¹d wylatuje ogieñ (lufa/paszcza)
    public float shootInterval = 3.0f;  // Co ile sekund strzela

    private Coroutine shootingCoroutine;
    private Transform targetPlayer;     // Przechowujemy referencjê do gracza, ¿eby celowaæ

    // --- Implementacja BasePlant ---

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

    // --- Logika Strzelania ---

    IEnumerator ShootRoutine()
    {
        // Ma³e opóŸnienie na start, ¿eby gracz nie dosta³ od razu po wejœciu
        yield return new WaitForSeconds(0.5f);

        while (targetPlayer != null)
        {
            ShootAtPlayer();
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootAtPlayer()
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        // 1. Tworzymy pocisk
        GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

        // 2. Celowanie w gracza
        // Obliczamy kierunek, ale ignorujemy wysokoœæ (Y), ¿eby ogieñ lecia³ p³asko po ziemi
        Vector3 targetPos = new Vector3(targetPlayer.position.x, spawnPoint.position.y, targetPlayer.position.z);

        proj.transform.LookAt(targetPos);
    }
}