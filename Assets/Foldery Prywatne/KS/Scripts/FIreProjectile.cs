using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 8.0f;
    public float maxLifetime = 4.0f;

    [Header("Efekt Podpalenia")]
    [Tooltip("Ile sekund gracz bêdzie p³on¹³ po trafieniu tym pociskiem.")]
    public float burnDuration = 3.0f;

    void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        // Ruch pocisku ZAWSZE do przodu wzglêdem swojej rotacji
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- ZMIANA: Logika podpalenia ---
            // Szukamy skryptu PlayerBurnStatus na obiekcie gracza
            PlayerBurnStatus burnStatus = other.GetComponent<PlayerBurnStatus>();

            if (burnStatus != null)
            {
                // Jeœli gracz ma ten skrypt, podpalamy go
                burnStatus.ApplyBurn(burnDuration);
                Debug.Log($"Pocisk trafi³! Gracz podpalony na {burnDuration} sekund.");
            }
            else
            {
                // Zabezpieczenie: Jeœli zapomnia³eœ dodaæ skryptu PlayerBurnStatus, 
                // zadaj zwyk³e obra¿enia, ¿eby gracz nie czu³ siê nieœmiertelny.
                Debug.LogWarning("Gracz nie ma komponentu PlayerBurnStatus! Zadajê zwyk³e obra¿enia.");
                if (TimeManager.Instance != null)
                {
                    TimeManager.Instance.ModifyTime(-10f);
                }
            }

            Destroy(gameObject); // Pocisk znika po trafieniu
        }
        // Niszczymy pocisk na œcianach/przeszkodach (ignorujemy inne triggery i sam¹ roœlinê)
        else if (!other.isTrigger && !other.GetComponent<BurningPlant>())
        {
            Destroy(gameObject);
        }
    }
}
