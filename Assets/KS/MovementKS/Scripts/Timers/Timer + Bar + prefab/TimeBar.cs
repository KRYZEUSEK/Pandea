using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // U¿yj tego, jeœli masz TextMeshPro

public class TimeBar : MonoBehaviour
{
    [Header("Referencje G³ówne")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Image totalTimeBar; // T³o paska
    [SerializeField] private Image currentTimeBar; // Zielony pasek (g³ówny)
    [SerializeField] private GameObject timeBarWhole; // Ca³y obiekt paska (do ukrycia)

    [Header("Wizualizacja Obra¿eñ Czasu")]
    [Tooltip("Czerwony pasek, który wizualizuje utracony czas.")]
    [SerializeField] private Image damageTimeBar;

    [Tooltip("Szybkoœæ animacji czerwonego paska, gdy znika.")]
    [SerializeField] private float damageBarAnimationSpeed = 5f;


    // Ta zmienna bêdzie œledziæ, czy animacja obra¿eñ jest aktywna.
    private Coroutine damageAnimationCoroutine;

    private void Start()
    {
        // Sprawdzenie, czy TimeManager jest podpiêty
        if (timeManager == null)
        {
            // Spróbuj znaleŸæ TimeManager automatycznie, jeœli nie jest podpiêty
            timeManager = TimeManager.Instance;
            if (timeManager == null)
            {
                Debug.LogError("Time Manager reference not set in TimeBar and could not be found!");
                return;
            }
        }

        // Inicjalizacja pasków
        totalTimeBar.fillAmount = 1f;
        currentTimeBar.fillAmount = 1f;

        if (damageTimeBar != null)
        {
            damageTimeBar.fillAmount = 1f;
        }

        // Subskrybuj zdarzenie z TimeManagera
        timeManager.OnTimeModified += HandleTimeModification;


    }

    /// <summary>
    /// Anuluj subskrypcjê, gdy obiekt jest niszczony, aby unikn¹æ b³êdów pamiêci.
    /// </summary>
    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnTimeModified -= HandleTimeModification;
        }
    }

    /// <summary>
    /// Update jest wywo³ywany w ka¿dej klatce.
    /// </summary>
    private void Update()
    {
        if (timeManager == null) return;

        // Pobierz znormalizowany czas (wartoœæ od 0.0 do 1.0)
        float normalizedTime = timeManager.GetNormalizedTime();

        // 1. Zielony pasek zawsze pokazuje aktualny czas
        currentTimeBar.fillAmount = normalizedTime;

        // 2. Czerwony pasek jest "blokowany" do zielonego...
        // ...CHYBA ¯E korutyna animacji obra¿eñ jest aktywna!
        if (damageAnimationCoroutine == null)
        {
            // Brak animacji - trzymaj paski idealnie razem
            damageTimeBar.fillAmount = normalizedTime;
        }
        // Jeœli korutyna JEST aktywna, to ona sama kontroluje 'damageTimeBar.fillAmount'

        // 3. Ukryj ca³y pasek, jeœli czas siê skoñczy³
        if (timeManager.IsTimeUp())
        {
            timeBarWhole.SetActive(false);
        }
    }

    /// <summary>
    /// Ta funkcja jest automatycznie wywo³ywana przez zdarzenie 'OnTimeModified' z TimeManagera.
    /// </summary>
    private void HandleTimeModification(float amount)
    {
        // Reaguj tylko na negatywn¹ modyfikacjê (utratê czasu)
        if (amount < 0f)
        {
            
            if (damageTimeBar != null)
            {
                
                if (damageAnimationCoroutine != null)
                {
                    StopCoroutine(damageAnimationCoroutine);
                }
                damageAnimationCoroutine = StartCoroutine(AnimateDamageBar());
            }

        }
    }

    /// <summary>
    /// Korutyna, która przejmuje kontrolê nad czerwonym paskiem
    /// i animuje go p³ynnie do pozycji zielonego paska.
    /// </summary>
    private IEnumerator AnimateDamageBar()
    {
        yield return null;

        float targetFill = currentTimeBar.fillAmount;

        while (damageTimeBar.fillAmount > targetFill + 0.01f)
        {
            damageTimeBar.fillAmount = Mathf.Lerp(
                damageTimeBar.fillAmount, // Z...
                targetFill,             // Do...
                Time.unscaledDeltaTime * damageBarAnimationSpeed // Z prêdkoœci¹ (niezale¿n¹ od pauzy!)
            );
            yield return null; // Czekaj do nastêpnej klatki
        }

        // Koniec animacji - zablokuj paski z powrotem razem
        damageTimeBar.fillAmount = targetFill;
        damageAnimationCoroutine = null; // Zaznacz, ¿e skoñczyliœmy (oddaj kontrolê do Update)
    }

    /// <summary>
}

