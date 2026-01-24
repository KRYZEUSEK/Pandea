using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public event Action<float> OnTimeModified;

    [Header("Time Settings")]
    [SerializeField] private float maxTime = 100f;
    [SerializeField] public float countdownDuration = 10f;

    public float currentTime;
    private float countdownRate;
    private bool isActive = false;

    // --- NOWO��: Mno�nik czasu ---
    private float timeMultiplier = 1.0f;

    public float GetNormalizedTime() => currentTime / maxTime;

    private void Awake()
    {
        Instance = this;
        currentTime = maxTime;
        CalculateCountdownRate();
    }

    private void OnEnable()
    {
        currentTime = maxTime;
        isActive = true;
        timeMultiplier = 1.0f; // Reset przy w��czeniu
    }

    // ... (Reszta metod OnDisable, CalculateCountdownRate, SetCountdownDuration bez zmian) ...
    private void CalculateCountdownRate()
    {
        countdownRate = maxTime / countdownDuration;
    }

    public void ModifyTime(float amount)
    {
        currentTime = Mathf.Clamp(currentTime + amount, 0f, maxTime);
        OnTimeModified?.Invoke(amount);
    }

    // --- NOWO��: Metoda do ustawiania mno�nika ---
    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = multiplier;
        // Opcjonalnie: Tutaj mo�esz doda� event, np. �eby zmieni� kolor paska na fioletowy, gdy czas leci szybciej
    }

    private void Update()
    {
        if (!isActive) return;

        if (currentTime > 0f)
        {
            // --- ZMIANA: Mno�ymy przez timeMultiplier ---
            // Je�li multiplier to 1, czas leci normalnie.
            // Je�li multiplier to 2, czas leci 2x szybciej.
            float decay = countdownRate * timeMultiplier * Time.deltaTime;

            currentTime -= decay;
            currentTime = Mathf.Max(currentTime, 0f);
        }
    }
    public bool IsTimeUp() => currentTime <= 0f;
    public float GetCurrentTime() => currentTime;
    public void ResetTime() => currentTime = maxTime;
    public void StopTime() => isActive = false;
    public void StartTime() => isActive = true;
}

