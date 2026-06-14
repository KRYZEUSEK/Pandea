using UnityEngine;
using UIScripts.Popups;

[RequireComponent(typeof(Collider))]
public class StoryPopupTrigger : MonoBehaviour
{
    [Header("Ustawienia Triggera")]
    [Tooltip("Tag obiektu gracza, który ma aktywowaæ popup.")]
    public string playerTag = "Player";

    [Tooltip("Czy popup z tekstem ma siê pokazaæ tylko raz przy pierwszym podejœciu?")]
    public bool triggerOnlyOnce = true;

    [Tooltip("Opcjonalnie: Klawisz na klawiaturze wywo³uj¹cy popup (np. F)")]
    public KeyCode interactKey = KeyCode.F;

    [Header("Referencje")]
    [Tooltip("Referencja do komponentu PopupSlides, który zawiera tekst i grafiki do wyœwietlenia.")]
    public PopupSlides popupSlides;

    [Tooltip("Referencja do obiektu UI (przycisku lub tekstu), który ma siê pojawiæ, gdy gracz jest w pobli¿u.")]
    public GameObject interactPrompt;

    private bool hasBeenTriggered = false;
    private bool isPlayerInRange = false;

    private void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Opcja 1: Zabezpieczenie dla klawiatury. 
        // Jeœli gracz woli wcisn¹æ klawisz (np. "F") zamiast klikaæ myszk¹ w przycisk na ekranie.
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            TriggerPopup();
        }
    }

    // Opcja 2: Ta metoda zostanie wywo³ana przez przycisk "SprawdŸ" na ekranie
    public void TriggerPopup()
    {
        // Przerywamy, jeœli popup mia³ wyskoczyæ tylko raz i to ju¿ siê sta³o
        if (triggerOnlyOnce && hasBeenTriggered) return;

        if (popupSlides != null)
        {
            // Ukrywamy przycisk "SprawdŸ" i otwieramy du¿e okno fabularne
            if (interactPrompt != null) interactPrompt.SetActive(false);

            popupSlides.Show();
            hasBeenTriggered = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasBeenTriggered) return;

        // Jeœli to gracz, pokazujemy przycisk "SprawdŸ"
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Jeœli gracz odejdzie, ukrywamy przycisk "SprawdŸ"
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}