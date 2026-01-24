using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class PlayerBurnStatus : MonoBehaviour
{
    [Header("Konfiguracja Podpalenia")]
    [SerializeField] private float burnTimeMultiplier = 3.0f;
    [SerializeField] private ParticleSystem fireVFX;

    [Header("Post Processing v2 (Pulsowanie)")]
    public PostProcessVolume postProcessVolume;

    [Tooltip("Kolor winiety (np. ostry ró¿/czerwieñ).")]
    public Color burnColor = new Color(1f, 0.4f, 0.6f); // Bardziej intensywny ró¿

    [Header("Ustawienia Pulsowania")]
    [Tooltip("Najni¿sza wartoœæ winiety podczas pulsowania.")]
    [Range(0f, 1f)] public float minPulseIntensity = 0.45f;

    [Tooltip("Najwy¿sza wartoœæ winiety (szczyt pulsu).")]
    [Range(0f, 1f)] public float maxPulseIntensity = 0.75f;

    [Tooltip("Jak szybko efekt pulsuje (iloœæ pulsów na sekundê * PI).")]
    public float pulseSpeed = 4f;

    [Tooltip("Jak szybko efekt znika po zgaszeniu.")]
    public float fadeOutSpeed = 2f;

    // Prywatne zmienne
    private Vignette _vignette;
    private Coroutine _burnCoroutine;
    private Coroutine _vignetteCoroutine; // Jedna korutyna do obs³ugi FX

    void Start()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindObjectOfType<PostProcessVolume>();

        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out _vignette))
        {
            _vignette.enabled.Override(true);
            _vignette.intensity.overrideState = true;
            _vignette.color.overrideState = true;

            _vignette.intensity.value = 0f;
            _vignette.color.value = burnColor;
        }
        else
        {
            Debug.LogWarning("Brak efektu Vignette w profilu Post Process!");
        }
    }

    public void ApplyBurn(float duration)
    {
        // Restartujemy logikê podpalenia
        if (_burnCoroutine != null) StopCoroutine(_burnCoroutine);
        _burnCoroutine = StartCoroutine(BurnLogicRoutine(duration));

        // Restartujemy efekt wizualny (pulsowanie)
        if (_vignetteCoroutine != null) StopCoroutine(_vignetteCoroutine);
        _vignetteCoroutine = StartCoroutine(PulseEffectRoutine());
    }

    // Logika czasu i mechaniki gry
    private IEnumerator BurnLogicRoutine(float duration)
    {
        if (fireVFX) fireVFX.Play();
        if (TimeManager.Instance) TimeManager.Instance.SetTimeMultiplier(burnTimeMultiplier);

        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        StopBurn();
    }

    private void StopBurn()
    {
        if (TimeManager.Instance) TimeManager.Instance.SetTimeMultiplier(1.0f);
        if (fireVFX) fireVFX.Stop();

        // Prze³¹czamy efekt wizualny na zanikanie
        if (_vignetteCoroutine != null) StopCoroutine(_vignetteCoroutine);
        _vignetteCoroutine = StartCoroutine(FadeOutRoutine());

        _burnCoroutine = null;
    }

    // --- KORUTYNA PULSOWANIA (SERCE SKRYPTU) ---
    private IEnumerator PulseEffectRoutine()
    {
        if (_vignette == null) yield break;

        // Ustawiamy kolor
        _vignette.color.value = burnColor;

        float t = 0f;

        // Pêtla nieskoñczona (dzia³a dopóki nie zostanie zatrzymana przez StopBurn)
        while (true)
        {
            t += Time.deltaTime * pulseSpeed;

            // Wzór na "ping-pong" miêdzy 0 a 1 przy u¿yciu Sinusa
            // Mathf.Sin zwraca od -1 do 1. 
            // (Sin + 1) / 2 zmienia to na zakres 0 do 1.
            float wave = (Mathf.Sin(t) + 1f) / 2f;

            // Lerp miesza minPulse i maxPulse w oparciu o falê
            float currentIntensity = Mathf.Lerp(minPulseIntensity, maxPulseIntensity, wave);

            _vignette.intensity.value = currentIntensity;

            yield return null;
        }
    }

    // --- KORUTYNA ZANIKANIA ---
    private IEnumerator FadeOutRoutine()
    {
        if (_vignette == null) yield break;

        float startIntensity = _vignette.intensity.value;
        float currentIntensity = startIntensity;

        while (currentIntensity > 0.01f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, 0f, Time.deltaTime * fadeOutSpeed);
            _vignette.intensity.value = currentIntensity;
            yield return null;
        }

        _vignette.intensity.value = 0f;
    }
}