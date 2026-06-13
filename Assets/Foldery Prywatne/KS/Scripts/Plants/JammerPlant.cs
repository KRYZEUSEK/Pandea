using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JammerPlant : BasePlant
{
    [Header("Ustawienia Interakcji")]
    public float jamRadius = 15f;

    [Header("Mechanika Usychania")]
    [Tooltip("Ile sekund roĹ›lina moĹĽe byÄ‡ aktywna zanim uschnie.")]
    public float maxActiveTime = 10f;
    private float currentActiveTimer = 0f;

    [Header("Wizualizacja ZasiÄ™gu")]
    [Tooltip("Obiekt baĹ„ki (np. pĂłĹ‚przezroczystej sfery) obrazujÄ…cy zasiÄ™g dziaĹ‚ania.")]
    public GameObject rangeVisual;

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

        // Skalujemy baĹ„kÄ™ na starcie do rozmiaru jamRadius
        if (rangeVisual != null)
        {
            // Kompensujemy skalÄ™ rodzica, aby baĹ„ka miaĹ‚a dokĹ‚adny rozmiar w przestrzeni Ĺ›wiata (World Space)
            Vector3 parentScale = transform.lossyScale;
            float targetScale = jamRadius * 2f;

            rangeVisual.transform.localScale = new Vector3(
                parentScale.x != 0 ? targetScale / parentScale.x : targetScale,
                parentScale.y != 0 ? targetScale / parentScale.y : targetScale,
                parentScale.z != 0 ? targetScale / parentScale.z : targetScale
            );
            
            // Ustawiamy stan baĹ„ki zgodnie z aktywnoĹ›ciÄ… jammera
            rangeVisual.SetActive(isJammerActive);
        }
    }

    protected override void OnPlayerEnter(GameObject player)
    {
        isPlayerInRange = true;
        
        // Aktywujemy jammer automatycznie po wejĹ›ciu gracza w trigger roĹ›liny
        if (!isJammerActive)
        {
            ActivateJammer();
        }
    }

    protected override void OnPlayerExit(GameObject player)
    {
        isPlayerInRange = false;
    }

    void Update()
    {
        // ObsĹ‚uga timera usychania
        if (isJammerActive)
        {
            currentActiveTimer += Time.deltaTime;

            if (currentActiveTimer >= maxActiveTime)
            {
                WitherAndDie();
            }
        }
    }

    private void ActivateJammer()
    {
        isJammerActive = true;
        if (pollenParticles != null) pollenParticles.Play();
        if (rangeVisual != null) rangeVisual.SetActive(true);

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
        Debug.Log("<color=yellow>Jammer aktywowany.</color> PozostaĹ‚y czas: " + (maxActiveTime - currentActiveTimer).ToString("F1") + "s");
    }

    private void DeactivateJammer()
    {
        isJammerActive = false;
        if (pollenParticles != null) pollenParticles.Stop();
        if (rangeVisual != null) rangeVisual.SetActive(false);

        foreach (var plant in affectedPlants)
        {
            if (plant != null) plant.SetPlantActive(true);
        }
        affectedPlants.Clear();
        Debug.Log("<color=grey>Jammer wyĹ‚Ä…czony.</color> Timer wstrzymany na: " + currentActiveTimer.ToString("F1") + "s");
    }

    private void WitherAndDie()
    {
        Debug.Log("<color=red>ROĹšLINA USCHĹA!</color>");

        // 1. PrzywrĂłÄ‡ inne roĹ›liny zanim ten obiekt zniknie
        DeactivateJammer();

        // 2. Efekt wizualny Ĺ›mierci
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 3. UsuĹ„ roĹ›linÄ™ ze Ĺ›wiata
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isJammerActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, jamRadius);
    }
}
