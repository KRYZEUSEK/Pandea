using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing; // Musisz dodaæ tê liniê!

public class BlindingPlant : BasePlant
{
    [Header("Post Processing")]
    public PostProcessVolume postProcessVolume; // Przypisz tu obiekt GlobalPostProcess

    [Header("Ustawienia")]
    public float fadeDuration = 0.5f; // Jak szybko wchodzimy w "œlepotê"

    private Coroutine visionCoroutine;


    new private void Awake() // Lub Start()
    {
        // Zabezpieczenie: Na starcie gry zawsze zerujemy efekt, 
        // ¿eby gracz nie zaczyna³ gry "œlepy", jeœli zapomnieliœmy przestawiæ suwak.
        if (postProcessVolume != null)
        {
            postProcessVolume.weight = 0f;
        }
    }

    // --- Wejœcie: W³¹czamy efekt (Weight d¹¿y do 1) ---
    protected override void OnPlayerEnter(GameObject player)
    {
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
        if (postProcessVolume == null)
        {
            Debug.LogError("Nie przypisa³eœ Post Process Volume!");
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