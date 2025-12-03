using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using System; 

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    // --- KROK 2: Deklaracja zdarzenia ---
    public event Action<float> OnTimeModified;

    [Header("Time Settings")]
    [SerializeField] private float maxTime = 100f; // Upewnij siê, ¿e to jest 100, a nie 10!
    [SerializeField] public float countdownDuration = 10f;

    private float currentTime;
    private float countdownRate;
    private bool isActive = false;

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
    }

    private void OnDisable()
    {
        isActive = false;
        currentTime = maxTime;
    }

    private void CalculateCountdownRate()
    {
        countdownRate = maxTime / countdownDuration;
    }

    public void SetCountdownDuration(float duration)
    {
        countdownDuration = Mathf.Max(0.1f, duration);
        CalculateCountdownRate();
    }

    public void ModifyTime(float amount)
    {
        currentTime = Mathf.Clamp(currentTime + amount, 0f, maxTime);

        // --- KROK 3: Wywo³anie zdarzenia ---
        // Ta linia "krzyczy" do TimeBara, ¿e czas siê zmieni³
        OnTimeModified?.Invoke(amount);
    }

    private void Update()
    {
        if (!isActive) return;

        if (currentTime > 0f)
        {
            currentTime -= countdownRate * Time.deltaTime;
            currentTime = Mathf.Max(currentTime, 0f);
        }
    }

    public bool IsTimeUp() => currentTime <= 0f;
    public float GetCurrentTime() => currentTime;
    public void ResetTime() => currentTime = maxTime;
    public void StopTime() => isActive = false;
    public void StartTime() => isActive = true;
}

