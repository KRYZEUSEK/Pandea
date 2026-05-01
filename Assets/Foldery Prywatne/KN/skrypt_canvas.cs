using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class skrypt_canvas : MonoBehaviour
{
    [Header("Ustawienia Kodu")]
    public int[] correctCode = { 4, 7, 5 };
    private int[] currentCode = { 0, 0, 0 };

    [Header("Referencje UI")]
    public TextMeshProUGUI[] digitTexts; // Przeciągnij tu 3 obiekty tekstowe z Canvasu
    
    // Ta funkcja będzie wywoływana przez przyciski pod cyframi
    public void IncrementDigit(int index)
    {
        currentCode[index]++;
        if (currentCode[index] > 9) 
        {
            currentCode[index] = 0; // Zapętlenie po 9 wraca do 0
        }
        UpdateUI();
    }

    // Ta funkcja będzie wywoływana przez przycisk "Sprawdź"
    public void CheckCode()
    {
        if (currentCode[0] == correctCode[0] &&
            currentCode[1] == correctCode[1] &&
            currentCode[2] == correctCode[2])
        {
            Debug.Log("Kod poprawny! Gra toczy się dalej.");
            ClosePuzzle();
            // TUTAJ DODAJ KOD NA NAGRODĘ (np. dodanie przedmiotu, otwarcie skrzynki)
        }
        else
        {
            Debug.Log("Błędny kod! Reset.");
            ResetPuzzle();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].text = currentCode[i].ToString();
        }
    }

    private void ResetPuzzle()
    {
        for (int i = 0; i < 3; i++)
        {
            currentCode[i] = 0;
        }
        UpdateUI();
    }

    // Wywoływane też z przycisku "Zamknij/Wyjdź", jeśli gracz chce zrezygnować
    public void ClosePuzzle() 
    {
        gameObject.SetActive(false);
        
        // Zamykamy kursor i wracamy do gry
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // UWAGA: Tutaj włącz z powrotem poruszanie się gracza, jeśli je wyłączyłeś.
    }

    private void OnEnable()
    {
        // Resetuje zagadkę za każdym razem, gdy gracz ją otwiera
        ResetPuzzle();
    }
}
