using UnityEngine;

public class PandaHealthIndicator : MonoBehaviour
{
    [Header("Grafiki Stanu Pandki")]
    [Tooltip("Obiekt graficzny wyświetlany gdy życie jest > 75%")]
    [SerializeField] private GameObject zielona;

    [Tooltip("Obiekt graficzny wyświetlany gdy życie jest od 50% do 75%")]
    [SerializeField] private GameObject zolta;

    [Tooltip("Obiekt graficzny wyświetlany gdy życie jest < 50%")]
    [SerializeField] private GameObject czerwona;

    private void Update()
    {
        // Sprawdzamy czy TimeManager istnieje na scenie
        if (TimeManager.Instance == null) return;

        // Pobieramy wartość życia (od 0.0 do 1.0) i przeliczamy na procenty
        float healthPercent = TimeManager.Instance.GetNormalizedTime() * 100f;

        // Określamy stany na podstawie procentów
        if (healthPercent >= 50f)
        {
            SetIndicatorState(true, false, false); // Tylko zielona
        }
        else if (healthPercent >= 25f && healthPercent < 50f)
        {
            SetIndicatorState(false, true, false); // Tylko żółta
        }
        else // healthPercent < 25%
        {
            SetIndicatorState(false, false, true); // Tylko czerwona
        }
    }

    private void SetIndicatorState(bool greenActive, bool yellowActive, bool redActive)
    {
        // Optymalizacja: zmieniamy SetActive tylko, gdy aktualny stan różni się od pożądanego
        if (zielona != null && zielona.activeSelf != greenActive) 
            zielona.SetActive(greenActive);
            
        if (zolta != null && zolta.activeSelf != yellowActive) 
            zolta.SetActive(yellowActive);
            
        if (czerwona != null && czerwona.activeSelf != redActive) 
            czerwona.SetActive(redActive);
    }
}
