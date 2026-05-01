using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skrypt_pudelka : MonoBehaviour
{
    [Header("UI Zagadki")]
    public GameObject puzzleCanvas; // Przeciągnij tu swój Canvas z zagadką

    private bool isPlayerNear = false;

    void Update()
    {
        // Jeśli gracz jest blisko, wciska E, a canvas nie jest jeszcze aktywny
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !puzzleCanvas.activeSelf)
        {
            OpenPuzzleUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Upewnij się, że twój gracz ma tag "Player"
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            // Opcjonalnie: zamknij UI, jeśli gracz odejdzie
            if (puzzleCanvas.activeSelf) puzzleCanvas.SetActive(false);
        }
    }

    private void OpenPuzzleUI()
    {
        puzzleCanvas.SetActive(true);
        
        // Odblokowujemy kursor, żeby gracz mógł klikać po UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // UWAGA: Tutaj warto dodać linijkę wyłączającą poruszanie się gracza/kamery,
        // jeśli masz jakiś skrypt od tego, np. PlayerMovement.enabled = false;
    }
}
