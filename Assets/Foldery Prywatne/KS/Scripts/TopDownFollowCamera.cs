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

    // Zmienna przechowuj¹ca sta³e przesuniêcie kamery w przestrzeni œwiata
    private Vector3 offset;

    void Start()
    {
        if (target == null) return;

        // Ustalamy sta³¹ pozycjê wzglêdem œwiata (Z i Y)
        offset = new Vector3(0, height, -distance);

        // Ustawiamy kamerê natychmiast na start, ¿eby nie "lecia³a" z punktu 0,0,0
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. DOCELOWA POZYCJA (Pozycja gracza + nasz sztywny offset)
        // Zauwa¿, ¿e nie ma tu ju¿ ¿adnych rotacji gracza (target.forward)
        Vector3 targetPosition = target.position + offset;

        // 2. P£YNNE PRZESUWANIE (Lerp)
        // To zniweluje wszelkie szarpania wynikaj¹ce z fizyki (wchodzenie na krzaki itp.)
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionDamping);

        // 3. KAMERA ZAWSZE PATRZY NA GRACZA
        transform.LookAt(target.position);
    }
}