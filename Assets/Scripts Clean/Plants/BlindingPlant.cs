using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing; // Musisz dodaæ tê liniê!

public class BlindingPlant : BasePlant
{
    [Header("Post Processing")]
    [Tooltip("Dok³adna nazwa obiektu na scenie z efektem œlepoty.")]
    public string postProcessObjectName = "BlindPostProcess"; // <-- ZMIANA: Szukamy po nazwie

    private PostProcessVolume postProcessVolume; // <-- ZMIANA: Skrypt sam wype³ni tê zmienn¹

    [Header("Ustawienia")]
    public float fadeDuration = 0.5f; // Jak szybko wchodzimy w "œlepotê"

    private Coroutine visionCoroutine;

    new private void Awake() // Lub Start()
    {
        // 1. Najpierw szukamy obiektu na scenie
        ZnajdzPostProcess();

        // 2. Zabezpieczenie: Na starcie gry zawsze zerujemy efekt
        if (postProcessVolume != null)
        {
            postProcessVolume.weight = 0f;
        }
    }

    // --- NOWA FUNKCJA: Szukanie obiektu po nazwie ---
    private void ZnajdzPostProcess()
    {
        // Szukamy aktywnego obiektu na scenie o podanej nazwie
        GameObject ppObject = GameObject.Find(postProcessObjectName);

        if (ppObject != null)
        {
            // Pobieramy komponent z tego obiektu
            postProcessVolume = ppObject.GetComponent<PostProcessVolume>();

            if (postProcessVolume == null)
            {
                Debug.LogError($"BlindingPlant: Znaleziono obiekt '{postProcessObjectName}', ale brakuje na nim komponentu PostProcessVolume!");
            }
        }
        else
        {
            Debug.LogError($"BlindingPlant: B£¥D! Nie znaleziono na scenie obiektu o nazwie '{postProcessObjectName}'.");
        }
    }

    // --- Wejœcie: W³¹czamy efekt (Weight d¹¿y do 1) ---
    protected override void OnPlayerEnter(GameObject player)
    {
        if (isDisabled) return;
        if (visionCoroutine != null) StopCoroutine(visionCoroutine);
        visionCoroutine = StartCoroutine(AnimateVignette(1f));
    }

    // --- Wyjœcie: Wy³¹czamy efekt (Weight d¹¿y do 0) ---
    protected override void OnPlayerExit(GameObject player)
    {
        if (visionCoroutine != null) StopCoroutine(visionCoroutine);
        visionCoroutine = StartCoroutine(AnimateVignette(0f));
    }

    IEnumerator AnimateVignette(float targetWeight)
    {
        // Jeœli skrypt nie znalaz³ obiektu na starcie, przerywamy korutynê
        if (postProcessVolume == null)
        {
            yield break;
        }

        float startWeight = postProcessVolume.weight;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            // P³ynna zmiana wagi efektu
            postProcessVolume.weight = Mathf.Lerp(startWeight, targetWeight, time / fadeDuration);
            yield return null;
        }

        postProcessVolume.weight = targetWeight;
    }

    // Zabezpieczenie: Jeœli wy³¹czysz grê bêd¹c w roœlinie, zresetuj efekt
    private void OnDisable()
    {
        if (postProcessVolume != null) postProcessVolume.weight = 0f;
    }
}