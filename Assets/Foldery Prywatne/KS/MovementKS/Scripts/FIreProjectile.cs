using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 10.0f;         // Prêdkoœæ lotu
    public float maxLifetime = 5.0f;    // Zabezpieczenie: zniszcz pocisk po 5s, ¿eby nie lecia³ w nieskoñczonoœæ

    [Header("Tworzenie Œciany")]
    public GameObject firePillarPrefab; // Prefab s³upa ognia (FirePillarHazard)
    public float spawnInterval = 0.5f;  // Co ile metrów stawiaæ s³up ognia (gêstoœæ œciany)

    private Vector3 lastSpawnPosition;

    void Start()
    {
        lastSpawnPosition = transform.position;
        Destroy(gameObject, maxLifetime); // Auto-zniszczenie "g³owicy" po czasie
    }

    void Update()
    {
        // 1. Ruch pocisku do przodu
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 2. Sprawdzamy dystans od ostatniego postawionego ognia
        float distanceTraveled = Vector3.Distance(transform.position, lastSpawnPosition);

        // Jeœli przelecieliœmy wystarczaj¹co daleko, stawiamy nowy s³up
        if (distanceTraveled >= spawnInterval)
        {
            SpawnFirePillar();
            lastSpawnPosition = transform.position;
        }
    }

    void SpawnFirePillar()
    {
        if (firePillarPrefab != null)
        {
            // Stawiamy ogieñ w aktualnej pozycji pocisku, z rotacj¹ 0 (prosto)
            Instantiate(firePillarPrefab, transform.position, Quaternion.identity);
        }
    }

    // Opcjonalnie: Zniszcz pocisk, jeœli w coœ uderzy (np. œcianê)
    void OnTriggerEnter(Collider other)
    {
        // Ignorujemy kolizjê z graczem (¿eby go nie blokowaæ) i samym ogniem
        if (!other.CompareTag("Player") && !other.GetComponent<FirePillarHazard>())
        {
            Destroy(gameObject);
        }
    }
}

