using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JammerPlant : BasePlant
{
    [Header("Ustawienia Interakcji")]
    public KeyCode interactionKey = KeyCode.I;
    public float jamRadius = 15f;

    [Header("Mechanika Usychania")]
    [Tooltip("Ile sekund roœlina mo¿e byæ aktywna zanim uschnie.")]
    public float maxActiveTime = 10f;
    private float currentActiveTimer = 0f;

    [Header("Efekty")]
    public ParticleSystem pollenParticles;
    [Tooltip("Opcjonalny efekt wizualny przy usychaniu (np. dym).")]
    public GameObject deathEffect;

    private bool isPlayerInRange = false;
    private bool isJammerActive = false;
    private List<BasePlant> affectedPlants = new List<BasePlant>();

    public override void Awake()
    {
        base.Awake();
        if (pollenParticles != null) pollenParticles.Stop();
    }

    protected override void OnPlayerEnter(GameObject player)
    {
        isPlayerInRange = true;
        Debug.Log("<color=cyan>Jammer:</color> Naciœnij " + interactionKey + " aby prze³¹czyæ pole.");
    }

    protected override void OnPlayerExit(GameObject player)
    {
        isPlayerInRange = false;
        // W tej wersji roœlina NIE deaktywuje siê sama po wyjœciu gracza, 
        // chyba ¿e chcesz, ¿eby timer bi³ tylko gdy gracz jest blisko.
    }

    void Update()
    {
        // 1. Obs³uga w³¹czania/wy³¹czania
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleJammer();
        }

        // 2. Obs³uga timera usychania
        if (isJammerActive)
        {
            currentActiveTimer += Time.deltaTime;

            // Opcjonalnie: loguj czas w konsoli co sekundê
            if (currentActiveTimer >= maxActiveTime)
            {
                WitherAndDie();
            }
        }
    }

    private void ToggleJammer()
    {
        if (!isJammerActive)
            ActivateJammer();
        else
            DeactivateJammer();
    }

    private void ActivateJammer()
    {
        isJammerActive = true;
        if (pollenParticles != null) pollenParticles.Play();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, jamRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (var col in hitColliders)
        {
            BasePlant plant = col.GetComponentInParent<BasePlant>();
            if (plant != null && plant != this)
            {
                if (!affectedPlants.Contains(plant))
                {
                    plant.SetPlantActive(false);
                    affectedPlants.Add(plant);
                }
            }
        }
        Debug.Log("<color=yellow>Jammer aktywowany.</color> Pozosta³y czas: " + (maxActiveTime - currentActiveTimer).ToString("F1") + "s");
    }

    private void DeactivateJammer()
    {
        isJammerActive = false;
        if (pollenParticles != null) pollenParticles.Stop();

        foreach (var plant in affectedPlants)
        {
            if (plant != null) plant.SetPlantActive(true);
        }
        affectedPlants.Clear();
        Debug.Log("<color=grey>Jammer wy³¹czony.</color> Timer wstrzymany na: " + currentActiveTimer.ToString("F1") + "s");
    }

    private void WitherAndDie()
    {
        Debug.Log("<color=red>ROŒLINA USCH£A!</color>");

        // 1. Przywróæ inne roœliny zanim ten obiekt zniknie
        DeactivateJammer();

        // 2. Efekt wizualny œmierci
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 3. Usuñ roœlinê ze œwiata
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isJammerActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, jamRadius);
    }
}