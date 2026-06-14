using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLogic : MonoBehaviour
{
    [Header("Ustawienia Logiki")]
    [SerializeField] private float timeToAddPerSecond = 5f;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float tickRate = 0.2f;

    [Header("Wizualizacja (Pulsowanie)")]
    [Tooltip("Przypisz tu obiekt wizualny (np. sferê bez collidera), która ma pulsowaæ")]
    [SerializeField] private Transform visualObject;
    [SerializeField] private float pulseSpeed = 5f;  // Jak szybko pulsuje
    [SerializeField] private float pulseStrength = 0.1f; // Jak mocno siê powiêksza

    // Zmienne wewnêtrzne
    private int objectsInRangeCount = 0; // Licznik zamiast boola (dla bezpieczeñstwa)
    private float timer = 0f;
    private Vector3 initialScale; // Zapamiêtujemy oryginalny rozmiar

    private void Start()
    {
        // Jeœli przypisano obiekt wizualny, zapamiêtaj jego pocz¹tkow¹ skalê
        if (visualObject != null)
        {
            initialScale = visualObject.localScale;
        }
    }

    private void Update()
    {
        // Obs³uga animacji pulsowania (w Update, bo to grafika)
        if (visualObject != null)
        {
            if (objectsInRangeCount > 0)
            {
                // Matematyka "bicia serca": Sinus czasu daje falê od -1 do 1
                float scaleChange = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;

                // Aplikujemy now¹ skalê: Baza + (1,1,1 * zmiana)
                visualObject.localScale = initialScale + (Vector3.one * scaleChange);
            }
            else
            {
                // Gdy gracza nie ma, wracamy p³ynnie do normalnego rozmiaru
                visualObject.localScale = Vector3.Lerp(visualObject.localScale, initialScale, Time.deltaTime * 5f);
            }
        }
    }

    private void FixedUpdate()
    {
        // Obs³uga dodawania czasu (w FixedUpdate, bo to logika gry)
        if (objectsInRangeCount > 0 && TimeManager.Instance != null)
        {
            timer += Time.deltaTime;

            if (timer >= tickRate)
            {
                float amountToAdd = timeToAddPerSecond * tickRate;
                TimeManager.Instance.ModifyTime(amountToAdd);
                timer -= tickRate;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    // --- Obs³uga Colliderów (Licznik) ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag)) objectsInRangeCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            objectsInRangeCount--;
            if (objectsInRangeCount < 0) objectsInRangeCount = 0;
        }
    }

    // Wersje 2D (jeœli u¿ywasz)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag)) objectsInRangeCount++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            objectsInRangeCount--;
            if (objectsInRangeCount < 0) objectsInRangeCount = 0;
        }
    }
}