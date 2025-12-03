using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonPlant : BasePlant
{
    [Header("Konfiguracja Trucizny")]
    [Tooltip("Ile dodatkowego czasu zabieramy na sekundê?")]
    public float extraDrainPerSecond = 20f;
    // JEŒLI Twój MaxTime to 100, a Duration to 10s (czyli normalnie schodzi 10/s),
    // to ustawiaj¹c tutaj 20/s uzyskasz ³¹cznie 30/s (czyli 3x szybciej).

    [Tooltip("Co ile sekund zadajemy obra¿enia? (Mniej = p³ynniej, Wiêcej = lepszy efekt paska)")]
    public float damageTickRate = 0.2f;

    [Header("Efekty")]
    public ParticleSystem cloudParticles;

    private Coroutine poisonCoroutine;

    // --- 1. Gracz wchodzi w chmurê ---
    protected override void OnPlayerEnter(GameObject player)
    {
        // Odpalamy wizualizacjê
        if (cloudParticles != null) cloudParticles.Play();

        // Zabezpieczenie przed podwójnym uruchomieniem
        if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);

        // Startujemy cykliczne odbieranie czasu
        poisonCoroutine = StartCoroutine(DrainTimeRoutine());
    }

    // --- 2. Gracz wychodzi z chmury ---
    protected override void OnPlayerExit(GameObject player)
    {
        // Zatrzymujemy wizualizacjê
        if (cloudParticles != null) cloudParticles.Stop();

        // Zatrzymujemy truciznê
        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
            poisonCoroutine = null;
        }
    }

    // --- 3. Logika zabierania czasu ---
    private IEnumerator DrainTimeRoutine()
    {
        // Obliczamy ile czasu zabraæ w jednym "tiku" (uderzeniu)
        // Np. Jeœli chcemy zabraæ 20 na sekundê, a uderzamy 5 razy na sekundê (0.2s),
        // to ka¿de uderzenie zabierze 4 punkty.
        float damagePerTick = extraDrainPerSecond * damageTickRate;

        while (true)
        {
            if (TimeManager.Instance != null && !TimeManager.Instance.IsTimeUp())
            {
                // U¿ywamy ModifyTime z minusem -> to uruchomi Czerwony Pasek w Twoim TimeBar
                TimeManager.Instance.ModifyTime(-damagePerTick);
            }

            // Czekamy do nastêpnego uderzenia
            yield return new WaitForSeconds(damageTickRate);
        }
    }
}
