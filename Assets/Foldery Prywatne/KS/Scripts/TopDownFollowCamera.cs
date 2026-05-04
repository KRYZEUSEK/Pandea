using UnityEngine;

public class TopDownFollowCamera : MonoBehaviour
{
    [Header("Cel i Odleg³oœci")]
    [HideInInspector]
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
        // We can leave this empty now, or keep it just in case you ever 
        // manually assign a target in the inspector for testing.
        if (target != null)
        {
            SetTarget(target);
        }
    }

    // --- NEW FUNCTION ---
    // Other scripts will call this to give the camera a target
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

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
        if (Input.GetMouseButton(2))
        {
            targetAngle += Input.GetAxis("Mouse X") * rotationSpeed;
        }

        // --- 2. P£YNNE PRZEJŒCIE K¥TA (Lerp) ---
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationDamping);

        // --- 3. AKTUALIZACJA POZYCJI KAMERY ---
        UpdateCameraPosition(false);
    }

    void UpdateCameraPosition(bool isInstant)
    {
        Vector3 baseOffset = new Vector3(0, height, -distance);
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        Vector3 rotatedOffset = rotation * baseOffset;
        Vector3 targetPosition = target.position + rotatedOffset;

        if (isInstant)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionDamping);
        }

        transform.LookAt(target.position);
    }
}