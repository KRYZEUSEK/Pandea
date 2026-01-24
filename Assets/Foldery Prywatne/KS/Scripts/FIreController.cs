using UnityEngine;

public class FireController : MonoBehaviour
{
    [Header("Ustawienia Ognia")]
    public ParticleSystem fireParticles;

    // Zapamiêtujemy oryginalne wartoœci z Inspectora
    private float baseEmissionRate;
    private float baseStartSpeed;

    [Header("Parametry Pulsowania")]
    public float pulseSpeed = 2.0f; // Jak szybko ogieñ "oddycha"
    public float pulseAmount = 10.0f; // O ile zmienia siê liczba cz¹steczek

    [Header("Interakcja (Test)")]
    public bool isMoving = false; // Czy ogieñ siê porusza (symulacja wiatru)

    void Start()
    {
        if (fireParticles == null)
            fireParticles = GetComponent<ParticleSystem>();

        // Pobieramy wartoœæ Emission -> Rate over Time z Twojego screena (50)
        baseEmissionRate = fireParticles.emission.rateOverTime.constant;
        baseStartSpeed = fireParticles.main.startSpeed.constant;
    }

    void Update()
    {
        // 1. Efekt "Oddychania" ognia (losowe migotanie + sinusoida)
        // Ogieñ nigdy nie pali siê jednostajnie.
        float noise = Mathf.PerlinNoise(Time.time * pulseSpeed, 0) * pulseAmount;
        float newEmission = baseEmissionRate + noise;

        // Dostêp do modu³u Emission
        var emissionModule = fireParticles.emission;
        emissionModule.rateOverTime = newEmission;

        // 2. Reakcja na ruch / "Wiatr"
        // Jeœli obiekt siê porusza, p³omieñ powinien byæ mniejszy (zdmuchiwany) lub bardziej chaotyczny
        if (isMoving)
        {
            // Zwiêkszamy prêdkoœæ wylotow¹ (Start Speed), ¿eby ogieñ by³ bardziej "agresywny"
            var mainModule = fireParticles.main;
            mainModule.startSpeed = baseStartSpeed * 1.5f;
        }
        else
        {
            // Powrót do normy
            var mainModule = fireParticles.main;
            mainModule.startSpeed = baseStartSpeed;
        }

        // TEST: Naciœnij SPACJÊ, aby zrobiæ "Wybuch"
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FlareUp();
        }
    }

    // Metoda do nag³ego buchniêcia ogniem
    public void FlareUp()
    {
        fireParticles.Emit(50); // Wypuœæ natychmiast 50 dodatkowych cz¹steczek
    }
}