using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 8.0f;      // Prêdkoœæ fali (mo¿e byæ wolniejsza, by ³atwiej przeskoczyæ)
    public float maxLifetime = 4.0f;

    // [Header("Tworzenie Œciany")] 
    // UWAGA: Wy³¹czy³em te pola, aby gracz móg³ przeskoczyæ pocisk i wyl¹dowaæ bezpiecznie.
    // Jeœli w³¹czysz tworzenie œciany w okrêgu 360 stopni, gracz wyl¹duje w ogniu.
    // public GameObject firePillarPrefab; 
    // public float spawnInterval = 0.5f;  
    // private Vector3 lastSpawnPosition;

    void Start()
    {
        // lastSpawnPosition = transform.position;
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        // 1. Ruch pocisku ZAWSZE do przodu wzglêdem swojej rotacji
        // Dziêki temu, ¿e w BurningPlant obróciliœmy je o 360 stopni, ka¿dy poleci w swoj¹ stronê na zewn¹trz.
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // --- SEKCJA TWORZENIA ŒCIANY (OPCJONALNA - ZAKOMENTOWANA DLA MECHANIKI SKAKANIA) ---
        /*
        float distanceTraveled = Vector3.Distance(transform.position, lastSpawnPosition);
        if (distanceTraveled >= spawnInterval)
        {
             SpawnFirePillar();
             lastSpawnPosition = transform.position;
        }
        */
    }

    /*
    void SpawnFirePillar()
    {
        if (firePillarPrefab != null)
        {
            Instantiate(firePillarPrefab, transform.position, Quaternion.identity);
        }
    }
    */

    void OnTriggerEnter(Collider other)
    {
        // Jeœli uderzy w gracza -> zadaj obra¿enia (zak³adam, ¿e masz skrypt TimeManager lub Health)
        if (other.CompareTag("Player"))
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ModifyTime(-10f); // Przyk³adowe obra¿enia
            }
            Destroy(gameObject); // Pocisk znika po trafieniu gracza
        }
        // Niszczymy pocisk na œcianach/przeszkodach (ale nie na Triggerach np. strefy roœliny)
        else if (!other.isTrigger && !other.GetComponent<BurningPlant>())
        {
            Destroy(gameObject);
        }
    }
}
