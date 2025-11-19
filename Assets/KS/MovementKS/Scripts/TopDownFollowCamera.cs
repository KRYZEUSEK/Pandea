using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownFollowCamera : MonoBehaviour
{
    [Header("Cel i Odleg³oœci")]
    [Tooltip("Obiekt transform motocykla, za którym ma pod¹¿aæ kamera.")]
    public Transform target;
    [Tooltip("Dystans kamery od motocykla.")]
    public float distance = 6f;
    [Tooltip("Wysokoœæ kamery nad motocyklem.")]
    public float height = 2.5f;

    [Header("P³ynnoœæ i OpóŸnienie")]
    [Tooltip("Szybkoœæ p³ynnego pod¹¿ania za pozycj¹ motocykla.")]
    public float positionDamping = 5f;
    [Tooltip("Szybkoœæ p³ynnego obracania siê kamery (Yaw).")]
    public float rotationDamping = 3f;

    [Header("Pochylenie Kamery (Roll)")]
    [Tooltip("Maksymalny k¹t pochylenia kamery (Roll) w reakcji na przechylenie motoru.")]
    public float maxCameraRoll = 10f;
    [Tooltip("Wspó³czynnik pochylenia kamery (im wiêkszy, tym kamera bardziej siê przechyla).")]
    public float rollFactor = 0.5f;
    [Tooltip("Szybkoœæ t³umienia pochylenia kamery.")]
    public float rollDamping = 8f;

    private Rigidbody targetRb;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Brak zdefiniowanego celu (target) dla kamery motocykla!");
            enabled = false;
            return;
        }

        targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null)
        {
            Debug.LogWarning("Motor nie ma komponentu Rigidbody. Ruch kamery mo¿e byæ mniej dynamiczny.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. OBLICZANIE DOCELOWEJ POZYCJI

        // Docelowa pozycja kamery to pozycja motocykla, przesuniêta o wektor 'za' i 'nad' nim.
        Vector3 docelowaPozycja = target.position - target.forward * distance + target.up * height;

        // P³ynne pod¹¿anie za pozycj¹
        transform.position = Vector3.Lerp(transform.position, docelowaPozycja, Time.deltaTime * positionDamping);

        // 2. OBLICZANIE DOCELOWEJ ROTACJI (Yaw/Pitch)

        // Docelowa rotacja: kamera patrzy na punkt nieco przed motocyklem.
        Quaternion docelowaRotacja = Quaternion.LookRotation(target.position + target.forward * 5f - transform.position);

        // P³ynne obracanie (Yaw)
        transform.rotation = Quaternion.Slerp(transform.rotation, docelowaRotacja, Time.deltaTime * rotationDamping);


        // 3. POCHYLENIE KAMERY (Roll)

        // Kamera delikatnie pochyla siê, reaguj¹c na pochylenie motocykla (lub ruch boczny)
        float currentTargetRoll = target.localEulerAngles.z;
        if (currentTargetRoll > 180)
            currentTargetRoll -= 360;

        // Oblicz docelowy Roll kamery (Roll motoru * wspó³czynnik)
        float docelowyRoll = -currentTargetRoll * rollFactor;

        // Ograniczenie maksymalnego k¹ta pochylenia kamery
        docelowyRoll = Mathf.Clamp(docelowyRoll, -maxCameraRoll, maxCameraRoll);

        // Pozycja Roll kamery (Z) jest niezale¿na od reszty obrotu (Yaw/Pitch)
        Quaternion rollRotacja = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            docelowyRoll
        );

        // P³ynna interpolacja Roll
        transform.rotation = Quaternion.Slerp(transform.rotation, rollRotacja, Time.deltaTime * rollDamping);
    }
}