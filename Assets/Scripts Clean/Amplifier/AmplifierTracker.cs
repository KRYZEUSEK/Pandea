using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AmplifierTracker : MonoBehaviour
{
    [Header("Ustawienia Celu")]
    public string targetTag = "Objective";

    [Header("Ustawienia Detekcji")]
    public float detectionRange = 100f;
    [Tooltip("D�ugo�� laserowego wska�nika.")]
    public float pointerLength = 5f;

    [Header("Czas Dzia�ania")]
    [Tooltip("Przez ile sekund wzmacniacz ma wskazywa� cel.")]
    public float activeDuration = 5f;

    [Header("Wygl�d Linii")]
    [Tooltip("Grubo�� linii na pocz�tku (przy wzmacniaczu).")]
    public float startWidth = 0.5f; // ZWI�KSZONO DOMY�LN� WARTO��
    [Tooltip("Grubo�� linii na ko�cu (na grocie wska�nika).")]
    public float endWidth = 0.0f;
    [Tooltip("Wysoko��, z kt�rej wylatuje laser (wzgl�dem �rodka obiektu).")]
    public float heightOffset = 1.0f; // NOWA ZMIENNA (zamiast sztywnego 0.5f)

    [Header("Efekt Migania (Stroboskop)")]
    [Tooltip("Szybko�� pulsowania linii.")]
    public float blinkSpeed = 8f;
    [Tooltip("G��wny kolor lasera (mo�esz w��czy� tu opcj� HDR w edytorze)")]
    public Color lineColor = Color.red;
    [Tooltip("Jak mocno linia ma �wieci� w szczytowym momencie b�ysku?")]
    public float maxGlowIntensity = 4f;

    private float currentActiveTime = 0f;
    private Transform currentTarget;
    private LineRenderer lineRenderer;
    private bool isDeployed = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;

        // Ustawienie gruboci pobrane ze zmiennych publicznych
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }

    void Start()
    {
        // Jeśli wzmacniacz jest stawiany bezpośrednio na scenie (np. przez system budowania na B),
        // nikt nie wywołuje na nim bezpośrednio metody Deploy(). Dlatego odpalamy go automatycznie.
        if (!isDeployed)
        {
            Deploy();
        }
    }

    public void Deploy()
    {
        isDeployed = true;
        lineRenderer.enabled = true;
        currentActiveTime = 0f; // Resetujemy timer
        FindNearestTarget();
    }

    void Update()
    {
        if (!isDeployed) return;

        // --- DODATEK: Aktualizacja grubo�ci w czasie rzeczywistym ---
        // Przydatne, je�li zmieniasz warto�ci w Inspektorze podczas gry
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;

        // 1. Odliczanie czasu dzia�ania (np. 5 sekund)
        currentActiveTime += Time.deltaTime;
        if (currentActiveTime >= activeDuration)
        {
            TurnOffTracker();
            return;
        }

        // 2. Szukanie celu, je�li go zgubili�my
        if (currentTarget == null)
        {
            FindNearestTarget();
            if (currentTarget == null)
            {
                lineRenderer.enabled = false;
                return;
            }
        }

        lineRenderer.enabled = true;

        // 3. Rysowanie linii i animacja migania
        DrawPointer();
        BlinkEffect();
    }

    void TurnOffTracker()
    {
        isDeployed = false;
        lineRenderer.enabled = false;

        Debug.Log("<color=orange>Wzmacniacz wy��czy� si� po czasie.</color>");

        // Je�li wzmacniacz ma po wszystkim znikn��, odkomentuj poni�sz� lini�:
        // Destroy(gameObject); 
    }

    void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        float closestDistance = detectionRange;
        currentTarget = null;

        foreach (GameObject t in targets)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                currentTarget = t.transform;
            }
        }
    }

    void DrawPointer()
    {
        // Kierunek do celu (z p�ask� osi� Y, �eby linia nie ucieka�a w ziemi�/niebo)
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;

        // Punkt pocz�tkowy podniesiony o nasz nowy offset z Inspektora
        Vector3 startPos = transform.position + Vector3.up * heightOffset;
        Vector3 endPos = startPos + (direction * pointerLength);

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    void BlinkEffect()
    {
        // 1. Podstawa: p�ynny sinus od 0 do 1
        float sineWave = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

        // 2. MAGIA: Pot�gujemy wynik. B�ysk jest bardzo kr�tki i "agresywny".
        float sharpBlink = Mathf.Pow(sineWave, 4f);

        // 3. Mno�ymy nasz bazowy kolor przez intensywno�� (tworzymy mocny kolor HDR)
        Color glowingColor = lineColor * (sharpBlink * maxGlowIntensity);

        // Ustawiamy przezroczysto�� (Alpha), kt�ra te� mocno pulsuje
        glowingColor.a = sharpBlink;

        lineRenderer.startColor = glowingColor;

        // Ko�c�wka lasera zawsze g�adko zanika (Alpha = 0), ale dziedziczy blask
        Color currentEndColor = lineColor * (sharpBlink * maxGlowIntensity);
        currentEndColor.a = 0f;
        lineRenderer.endColor = currentEndColor;
    }
}