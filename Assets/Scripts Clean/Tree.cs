using UnityEngine;

// 1. USUNIÊTO: [RequireComponent(typeof(Collider))] - rodzic nie musi mieæ collidera
public class Tree : MonoBehaviour
{
    [Header("Uderzenia")]
    [SerializeField] private int maxHits = 2;
    [SerializeField] private float hitCooldown = 0.25f;

    [Header("Spawn po œciêciu")]
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private int logsToSpawn = 1;

    private int currentHits = 0;
    private float lastHitTime = -999f;
    private Collider col;

    private void Awake()
    {
        // 2. ZMIENIONO: Szukamy Collidera w obiekcie g³ównym ORAZ we wszystkich jego dzieciach
        col = GetComponentInChildren<Collider>();

        if (col == null)
        {
            Debug.LogWarning($"[Tree] Nie znaleziono Collidera w dzieciach obiektu {name}!");
        }
    }

    public void Hit()
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        currentHits++;

        if (currentHits >= maxHits)
        {
            ChopDown();
        }
    }

    private void ChopDown()
    {
        if (col != null) col.enabled = false;

        if (logPrefab != null)
        {
            for (int i = 0; i < logsToSpawn; i++)
            {
                Vector3 randomSmallOffset = spawnOffset + new Vector3(
                    Random.Range(-0.15f, 0.15f), 0f, Random.Range(-0.15f, 0.15f)
                );

                Instantiate(
                    logPrefab,
                    transform.position + randomSmallOffset,
                    Quaternion.identity
                );
            }
        }
        else
        {
            Debug.LogWarning($"[Tree] Brak przypiêtego logPrefab na {name}");
        }

        Destroy(gameObject); // To zniszczy ca³y obiekt drzewa razem z dzieæmi
    }
}