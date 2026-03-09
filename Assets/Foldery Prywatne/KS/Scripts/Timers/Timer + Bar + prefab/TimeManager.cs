using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public event Action<float> OnTimeModified;
    // --- NOWOŚĆ: Event informujący o końcu czasu ---
    public event Action OnTimeFinished;

    [Header("Time Settings")]
    [SerializeField] private float maxTime = 100f;
    [SerializeField] public float countdownDuration = 10f;

    public float currentTime;
    private float countdownRate;
    private bool isActive = false;
    private float timeMultiplier = 1.0f;

    // Zabezpieczenie, by event wywołał się tylko raz
    private bool hasTimeEnded = false;

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
        timeMultiplier = 1.0f;
        hasTimeEnded = false; // Resetujemy flagę końca czasu
    }

    private void CalculateCountdownRate()
    {
        countdownRate = maxTime / countdownDuration;
    }

    public void ModifyTime(float amount)
    {
        if (hasTimeEnded) return; // Jeśli gra się skończyła, nie dodajemy czasu

        currentTime = Mathf.Clamp(currentTime + amount, 0f, maxTime);
        OnTimeModified?.Invoke(amount);
    }

    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = multiplier;
    }

    private void Update()
    {
        // Jeśli czas nie leci lub już się skończył, nic nie robimy
        if (!isActive || hasTimeEnded) return;

        if (currentTime > 0f)
        {
            float decay = countdownRate * timeMultiplier * Time.deltaTime;
            currentTime -= decay;

            // --- SPRAWDZENIE KOŃCA CZASU ---
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                hasTimeEnded = true;
                isActive = false; // Zatrzymujemy czas

                // Krzyczymy do innych skryptów, że czas się skończył!
                OnTimeFinished?.Invoke();
            }
        }
    }

    public bool IsTimeUp() => currentTime <= 0f;
    public float GetCurrentTime() => currentTime;
    public void ResetTime() => currentTime = maxTime;
    public void StopTime() => isActive = false;
    public void StartTime() => isActive = true;
}