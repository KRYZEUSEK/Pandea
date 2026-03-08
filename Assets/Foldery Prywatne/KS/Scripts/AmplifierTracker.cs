using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AmplifierTracker : MonoBehaviour
{
    [Header("Ustawienia Celu")]
    public string targetTag = "Objective";

    [Header("Ustawienia Detekcji")]
    public float detectionRange = 100f;
    [Tooltip("D³ugoœæ laserowego wskaŸnika.")]
    public float pointerLength = 5f;

    [Header("Czas Dzia³ania")]
    [Tooltip("Przez ile sekund wzmacniacz ma wskazywaæ cel.")]
    public float activeDuration = 5f;

    [Header("Efekt Migania (Stroboskop)")]
    [Tooltip("Szybkoœæ pulsowania linii.")]
    public float blinkSpeed = 8f;
    public Color lineColor = Color.red;
    [Tooltip("Jak mocno linia ma œwieciæ w szczytowym momencie b³ysku?")]
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

        // Automatyczne ustawienie cienkiej linii (grubsza u nasady, zwê¿aj¹ca siê ku koñcowi)
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.0f;
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

        // 1. Odliczanie czasu dzia³ania (np. 5 sekund)
        currentActiveTime += Time.deltaTime;
        if (currentActiveTime >= activeDuration)
        {
            TurnOffTracker();
            return;
        }

        // 2. Szukanie celu, jeœli go zgubiliœmy
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

        // 3. Rysowanie linii i agresywna animacja migania
        DrawPointer();
        BlinkEffect();
    }

    void TurnOffTracker()
    {
        isDeployed = false;
        lineRenderer.enabled = false;

        Debug.Log("<color=orange>Wzmacniacz wy³¹czy³ siê po czasie.</color>");

        // Jeœli wzmacniacz ma po wszystkim znikn¹æ, odkomentuj poni¿sz¹ liniê:
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
        // Kierunek do celu (z p³ask¹ osi¹ Y, ¿eby linia nie ucieka³a w ziemiê/niebo)
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;

        // Punkt pocz¹tkowy podniesiony lekko nad ziemiê
        Vector3 startPos = transform.position + Vector3.up * 0.5f;
        Vector3 endPos = startPos + (direction * pointerLength);

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    void BlinkEffect()
    {
        // 1. Podstawa: p³ynny sinus od 0 do 1
        float sineWave = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

        // 2. MAGIA: Potêgujemy wynik. B³ysk jest bardzo krótki i "agresywny".
        float sharpBlink = Mathf.Pow(sineWave, 4f);

        // 3. Mno¿ymy nasz bazowy kolor przez intensywnoœæ (tworzymy mocny kolor HDR)
        Color glowingColor = lineColor * (sharpBlink * maxGlowIntensity);

        // Ustawiamy przezroczystoœæ (Alpha), która te¿ mocno pulsuje
        glowingColor.a = sharpBlink;

        lineRenderer.startColor = glowingColor;

        // Koñcówka lasera zawsze g³adko zanika (Alpha = 0), ale dziedziczy blask
        Color currentEndColor = lineColor * (sharpBlink * maxGlowIntensity);
        currentEndColor.a = 0f;
        lineRenderer.endColor = currentEndColor;
    }
}