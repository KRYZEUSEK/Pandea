using UnityEngine;

public class TopDownFollowCamera : MonoBehaviour
{
    [Header("Cel i Odleg³oœci")]
    public Transform target;
    [Tooltip("Jak daleko z ty³u ma byæ kamera (na osi Z)")]
    public float distance = 8f;
    [Tooltip("Jak wysoko nad graczem ma byæ kamera (na osi Y)")]
    public float height = 10f;

    [Header("P³ynnoœæ (Damping)")]
    [Tooltip("Jak szybko kamera dogania pozycjê gracza. Wiêcej = szybciej.")]
    public float positionDamping = 8f;

    [Header("Obrót Kamery (ŒPM)")]
    [Tooltip("Szybkoœæ obrotu kamery przy ruchu myszk¹.")]
    public float rotationSpeed = 5f;
    [Tooltip("P³ynnoœæ zatrzymywania obrotu (Damping). Wiêcej = sztywniej, mniej = du¿e 'œlizganie'.")]
    public float rotationDamping = 10f;

    // K¹ty obrotu
    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Start()
    {
        if (target == null) return;

        // Ustawiamy startowy k¹t na podstawie obecnej rotacji kamery w œwiecie
        currentAngle = transform.eulerAngles.y;
        targetAngle = currentAngle;

        // Wymuszamy natychmiastowe ustawienie kamery na start (bez p³ynnego dojazdu)
        UpdateCameraPosition(true);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- 1. OBS£UGA OBROTU MYSZK¥ ---
        // Input.GetMouseButton(2) to Œrodkowy Przycisk Myszki (kó³ko)
        if (Input.GetMouseButton(2))
        {
            // Zmieniamy docelowy k¹t na podstawie ruchu myszki w osi X (lewo/prawo)
            targetAngle += Input.GetAxis("Mouse X") * rotationSpeed;
        }

        // --- 2. P£YNNE PRZEJŒCIE K¥TA (Lerp) ---
        // Zapewnia to miêkki start i stop przy obracaniu kamery
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationDamping);

        // --- 3. AKTUALIZACJA POZYCJI KAMERY ---
        UpdateCameraPosition(false);
    }

    void UpdateCameraPosition(bool isInstant)
    {
        // Wyliczamy bazowy offset tak, jak robi³eœ to wczeœniej
        Vector3 baseOffset = new Vector3(0, height, -distance);

        // Tworzymy rotacjê wokó³ osi Y (w górê) o nasz wyliczony, p³ynny k¹t
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

        // Mno¿ymy rotacjê przez offset - to obraca nasz¹ pozycjê wokó³ gracza jak planetê!
        Vector3 rotatedOffset = rotation * baseOffset;

        // Nasza nowa, docelowa pozycja
        Vector3 targetPosition = target.position + rotatedOffset;

        if (isInstant)
        {
            // Natychmiastowe ustawienie (u¿ywane tylko w funkcji Start)
            transform.position = targetPosition;
        }
        else
        {
            // P³ynne pod¹¿anie za graczem, gdy ten idzie
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionDamping);
        }

        // Na koniec kamera zawsze musi patrzeæ w œrodek gracza
        transform.LookAt(target.position);
    }
}