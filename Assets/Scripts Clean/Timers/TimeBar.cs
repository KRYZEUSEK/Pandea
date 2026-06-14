using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeBar : MonoBehaviour
{
    [Header("Paski")]
    [SerializeField] private Image mainBar;     // G³ówny pasek (np. Bia³y)
    [SerializeField] private Image effectBar;   // Pasek efektów (zmienia kolor: Czerwony/Niebieski)

    [Header("Kolory")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.cyan; // Jasny niebieski

    [Header("Ustawienia")]
    [SerializeField] private float animationSpeed = 0.5f; // Czas trwania animacji

    private Coroutine activeCoroutine;

    private void Start()
    {
        // Subskrypcja do Twojego TimeManagera
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeModified += HandleTimeModified;
        }
        UpdateBarsInstant();
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeModified -= HandleTimeModified;
        }
    }

    private void Update()
    {
        // W Update aktualizujemy paski tylko jeœli NIE ma aktywnej animacji zmiany (dodania/odjêcia)
        // Dziêki temu pasek normalnie maleje wraz z up³ywem czasu (countdown)
        if (activeCoroutine == null && TimeManager.Instance != null)
        {
            float currentNorm = TimeManager.Instance.GetNormalizedTime();
            mainBar.fillAmount = currentNorm;
            effectBar.fillAmount = currentNorm;
        }
    }

    // --- TO JEST KLUCZOWA METODA ---
    private void HandleTimeModified(float amount)
    {
        // Zatrzymujemy poprzedni¹ animacjê, jeœli jakaœ trwa³a
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);

        float targetFill = TimeManager.Instance.GetNormalizedTime();

        if (amount > 0)
        {
            // === DODAWANIE CZASU (Niebieski Pasek) ===
            // 1. Zmieniamy kolor paska pod spodem na Niebieski
            effectBar.color = healColor;

            // 2. Pasek pod spodem od razu skacze do NOWEJ (wy¿szej) wartoœci
            effectBar.fillAmount = targetFill;

            // 3. G³ówny pasek zostaje na starej wartoœci i powoli roœnie
            // (Obliczamy star¹ wartoœæ na podstawie tego, gdzie pasek jest teraz)
            activeCoroutine = StartCoroutine(AnimateMainBarToTarget(targetFill));
        }
        else
        {
            // === ODEJMOWANIE CZASU (Czerwony Pasek) ===
            // 1. Zmieniamy kolor paska pod spodem na Czerwony
            effectBar.color = damageColor;

            // 2. G³ówny pasek od razu spada do NOWEJ (ni¿szej) wartoœci
            mainBar.fillAmount = targetFill;

            // 3. Czerwony pasek zostaje na starej (wysokiej) wartoœci i powoli spada
            // (W tym przypadku effectBar jest "powy¿ej" mainBar, wiêc go widaæ)
            activeCoroutine = StartCoroutine(AnimateEffectBarToTarget(targetFill));
        }
    }

    // Animacja przy DODAWANIU (G³ówny goni Niebieski)
    private IEnumerator AnimateMainBarToTarget(float target)
    {
        float startFill = mainBar.fillAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationSpeed;
            mainBar.fillAmount = Mathf.Lerp(startFill, target, t);
            yield return null;
        }

        mainBar.fillAmount = target;
        activeCoroutine = null; // Koniec animacji, wracamy do normalnego Update
    }

    // Animacja przy ODEJMOWANIU (Czerwony goni G³ówny)
    private IEnumerator AnimateEffectBarToTarget(float target)
    {
        float startFill = effectBar.fillAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationSpeed;
            effectBar.fillAmount = Mathf.Lerp(startFill, target, t);
            yield return null;
        }

        effectBar.fillAmount = target;
        activeCoroutine = null; // Koniec animacji
    }

    private void UpdateBarsInstant()
    {
        if (TimeManager.Instance == null) return;
        float norm = TimeManager.Instance.GetNormalizedTime();
        mainBar.fillAmount = norm;
        effectBar.fillAmount = norm;
    }
}