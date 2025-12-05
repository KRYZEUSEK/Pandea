using UnityEngine;

public class TopDownFollowCamera : MonoBehaviour
{
    [Header("Cel i Odleg³oœci")]
    public Transform target;
    public float distance = 8f;
    public float height = 6f;

    [Header("P³ynnoœæ Pod¹¿ania (Position)")]
    public float positionDamping = 4f; // Jak szybko kamera goni pozycjê
    public float rotationDamping = 2f; // Jak szybko kamera obraca siê do celu

    [Header("P³ynnoœæ Zmiany Kierunku (Nowoœæ)")]
    [Tooltip("Jak szybko kamera reaguje na zmianê kierunku ruchu gracza. Mniejsza wartoœæ = ³agodniejsze ³uki.")]
    public float directionSmoothing = 2.5f; // TO JEST KLUCZ DO NAPRAWY PRZESKOKU

    [Header("Logika Ruchu")]
    public float moveThreshold = 0.1f;

    // --- ZMIENNE PRYWATNE ---
    private Vector3 lastTargetPosition;
    private Vector3 currentHeading; // Wyg³adzony wektor kierunku ("plecy" gracza)

    void Start()
    {
        if (target == null) return;

        lastTargetPosition = target.position;
        currentHeading = target.forward;

        // Ustawienie startowe
        UpdateCameraPosition(true);
    }

    void LateUpdate()
    {
        if (target == null) return;
        UpdateCameraPosition(false);
    }

    void UpdateCameraPosition(bool instant)
    {
        // 1. OBLICZANIE RZECZYWISTEGO RUCHU
        Vector3 displacement = target.position - lastTargetPosition;
        Vector3 flatDisplacement = new Vector3(displacement.x, 0, displacement.z); // Ignorujemy Y
        float moveDistance = flatDisplacement.magnitude;

        // 2. AKTUALIZACJA KIERUNKU (Z WYG£ADZANIEM)
        if (moveDistance > moveThreshold)
        {
            Vector3 inputDirection = flatDisplacement.normalized;

            if (instant)
            {
                currentHeading = inputDirection;
            }
            else
            {
                // TUTAJ JEST NAPRAWA:
                // Zamiast: currentHeading = inputDirection;
                // U¿ywamy Slerp, aby wektor "pleców" obraca³ siê powoli, a nie przeskakiwa³.
                currentHeading = Vector3.Slerp(currentHeading, inputDirection, Time.deltaTime * directionSmoothing);
            }
        }

        // Zapisujemy pozycjê na nastêpn¹ klatkê
        lastTargetPosition = target.position;

        // 3. OBLICZANIE POZYCJI DOCELOWEJ
        // Pozycja jest obliczana na podstawie WYG£ADZONEGO wektora 'currentHeading'
        Vector3 targetPos = target.position - currentHeading * distance + Vector3.up * height;

        // 4. APLIKOWANIE RUCHU
        if (instant)
        {
            transform.position = targetPos;
            transform.LookAt(target.position);
        }
        else
        {
            // Lerp pozycji (t³umienie drgañ)
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * positionDamping);

            // LookAt z lekkim wyg³adzeniem (¿eby nie trzês³o przy mikro-ruchach)
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationDamping);
        }
    }
}