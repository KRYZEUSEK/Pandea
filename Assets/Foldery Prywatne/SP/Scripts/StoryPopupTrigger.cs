using UnityEngine;
using UIScripts.Popups;

[RequireComponent(typeof(Collider))]
public class StoryPopupTrigger : MonoBehaviour
{
    [Header("Ustawienia Triggera")]
    [Tooltip("Tag obiektu gracza, który ma aktywowaæ popup")]
    public string playerTag = "Player";

    [Tooltip("Czy popup z tekstem ma siê pokazaæ tylko raz przy pierwszym podejœciu")]
    public bool triggerOnlyOnce = true;

    [Header("Referencje")]
    [Tooltip("Referencja do komponentu PopupSlides, który zawiera tekst i grafiki do wyœwietlenia.")]
    public PopupSlides popupSlides;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasBeenTriggered) return;

        if (other.CompareTag(playerTag))
        {
            if (popupSlides != null)
            {
                popupSlides.Show();
                hasBeenTriggered = true;
            }
            else
            {
                Debug.LogWarning($"[StoryPopupTrigger] Brak przypisanego komponentu PopupSlides na obiekcie: {gameObject.name}");
            }
        }
    }
}