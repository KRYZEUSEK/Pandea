using UnityEngine;
using UIScripts.Popups; // Wymagane, aby widzieæ system Popupów

public class Interactable : MonoBehaviour
{
    [Tooltip("Wpisz tutaj dok³adnie takie samo ID, jakie nada³eœ w Special Slides w UI (np. 'WrakStatku')")]
    public string specialSlideId = "WrakStatku";

    public void TriggerInteraction()
    {
        // Sprawdzamy, czy system UI istnieje na scenie
        if (PopupSlides.Instance != null)
        {
            // Odpalamy specjalny slajd po jego ID!
            PopupSlides.Instance.ShowSpecialSlide(specialSlideId);
        }
        else
        {
            Debug.LogWarning("Brak PopupSlides.Instance na scenie! Upewnij siê, ¿e masz ten skrypt na Canvasie.");
        }
    }
}