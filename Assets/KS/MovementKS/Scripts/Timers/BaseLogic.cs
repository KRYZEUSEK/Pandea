using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLogic : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private float timeToAddPerSecond = 5f;
    [SerializeField] private string targetTag = "Player";

    // NOWE: Jak czêsto aktualizowaæ czas (np. co 0.2 sekundy)
    [SerializeField] private float tickRate = 0.2f;

    private bool isPlayerInRange = false;
    private float timer = 0f;

    private void Update()
    {
        if (isPlayerInRange && TimeManager.Instance != null)
        {
            // Zliczamy czas
            timer += Time.deltaTime;

            // Jeœli min¹³ ustalony czas (np. 0.2s)
            if (timer >= tickRate)
            {
                // Obliczamy ile czasu dodaæ za ten konkretny okres (0.2s * 5/s = 1 punkt)
                float amountToAdd = timeToAddPerSecond * tickRate; // Wa¿ne: mno¿ymy przez tickRate!

                TimeManager.Instance.ModifyTime(amountToAdd);

                // Resetujemy licznik (odejmujemy tickRate, ¿eby zachowaæ nadmiarowe milisekundy)
                timer -= tickRate;
            }
        }
        else
        {
            // Resetujemy timer, ¿eby po ponownym wejœciu nie doda³o od razu
            timer = 0f;
        }
    }

    // --- Obs³uga Colliderów bez zmian ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag)) isPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag)) isPlayerInRange = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag)) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag)) isPlayerInRange = false;
    }
}