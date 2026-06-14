using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BasePlant : MonoBehaviour
{
    [Header("Ustawienia Bazowe")]
    public Collider plantCollider;

    [Tooltip("Czy roœlina jest aktualnie wy³¹czona (np. przez Jammera).")]
    public bool isDisabled = false;

    public virtual void Awake()
    {
        // Jeœli nie przypisano collidera w inspektorze, pobierz go automatycznie
        if (plantCollider == null)
            plantCollider = GetComponent<Collider>();

        // Roœlina musi byæ triggerem, aby OnPlayerEnter dzia³a³o
        plantCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Jeœli roœlina jest uœpiona, ca³kowicie ignorujemy wejœcie gracza
        if (isDisabled) return;

        if (other.CompareTag("Player"))
        {
            OnPlayerEnter(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Wyjœcie gracza obs³ugujemy zawsze, aby posprz¹taæ ewentualne referencje
        if (other.CompareTag("Player"))
        {
            OnPlayerExit(other.gameObject);
        }
    }

    /// <summary>
    /// Metoda wywo³ywana przez JammerPlant. 
    /// Ca³kowicie w³¹cza lub wy³¹cza logikê skryptu.
    /// </summary>
    public virtual void SetPlantActive(bool active)
    {
        isDisabled = !active;

        if (isDisabled)
        {
            // 1. Natychmiast przerywamy wszystkie Coroutiny (np. serie strza³ów)
            StopAllCoroutines();

            // 2. Wywo³ujemy OnPlayerExit rêcznie, jeœli roœlina ma coœ "posprz¹taæ" 
            // (opcjonalne, zale¿nie od tego jak piszesz logikê w dzieciach)
            OnPlayerExit(null);

            // 3. Wy³¹czamy komponent, by zatrzymaæ Update()
            this.enabled = false;

            Debug.Log($"<color=red>{gameObject.name} zosta³a uœpiona.</color>");
        }
        else
        {
            // Przywracamy dzia³anie skryptu
            this.enabled = true;
            Debug.Log($"<color=green>{gameObject.name} zosta³a wybudzona.</color>");
        }
    }

    // Metody do nadpisania w konkretnych roœlinach (np. ShootSlowPlant)
    protected virtual void OnPlayerEnter(GameObject player) { }
    protected virtual void OnPlayerExit(GameObject player) { }
}