using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireLight : MonoBehaviour
{
    [Header("Ustawienia Jasnoœci")]
    public float minIntensity = 2f;      // Najciemniejszy moment
    public float maxIntensity = 4f;      // Najjaœniejszy moment
    [Tooltip("Jak szybko œwiat³o migocze")]
    public float flickerSpeed = 3.0f;    // Prêdkoœæ zmian

    [Header("Ruch Œwiat³a (Dla tañcz¹cych cieni)")]
    public bool enableMovement = true;   // Czy œwiat³o ma siê ruszaæ?
    public float moveRange = 0.1f;       // Jak daleko mo¿e siê przesun¹æ
    public float moveSpeed = 2.0f;       // Jak szybko siê przesuwa

    private Light fireLight;
    private float randomOffset;          // ¯eby ka¿dy ogieñ miga³ inaczej
    private Vector3 startPosition;       // Zapamiêtana pozycja startowa

    void Awake()
    {
        fireLight = GetComponent<Light>();
        startPosition = transform.position;
        // Losujemy offset, ¿eby dwa ogniska obok siebie nie miga³y identycznie
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (fireLight == null) return;

        // 1. Obliczanie jasnoœci przy u¿yciu Szumu Perlina (p³ynna losowoœæ)
        // Time.time * speed + offset sprawia, ¿e poruszamy siê po wykresie szumu
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);

        // Mathf.Lerp p³ynnie miesza min i max w zale¿noœci od szumu (0-1)
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // 2. Delikatny ruch Ÿród³a œwiat³a
        if (enableMovement)
        {
            float x = Mathf.PerlinNoise(Time.time * moveSpeed, randomOffset + 10);
            float y = Mathf.PerlinNoise(Time.time * moveSpeed, randomOffset + 20);
            float z = Mathf.PerlinNoise(Time.time * moveSpeed, randomOffset + 30);

            // Przesuwamy od -0.5 do 0.5 wzglêdem startowej pozycji
            Vector3 offset = new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) * moveRange;
            transform.position = startPosition + offset;
        }
    }
}
