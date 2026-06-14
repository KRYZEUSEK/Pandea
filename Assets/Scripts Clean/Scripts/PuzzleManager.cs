using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Ustawienia Prefabu")]
    public GameObject successCanvasPrefab;

    private PuzzleInteraction puzzleToWatch;
    private bool hasBeenTriggered = false;

    private void Update()
    {
        // 1. Jeœli zagadka zosta³a wygenerowana, ale jeszcze jej nie znaleŸliœmy:
        if (puzzleToWatch == null)
        {
            puzzleToWatch = Object.FindAnyObjectByType<PuzzleInteraction>();

        }

        // 2. Jeœli znaleŸliœmy zagadkê i jeszcze nie aktywowaliœmy nagrody:
        if (puzzleToWatch != null && !hasBeenTriggered)
        {
            // Sprawdzamy, czy oryginalny skrypt siê wy³¹czy³ (czyli zagadka zosta³a ukoñczona)
            if (!puzzleToWatch.enabled)
            {
                hasBeenTriggered = true; 
                SpawnSuccessCanvas();
            }
        }
    }

    private void SpawnSuccessCanvas()
    {
        if (successCanvasPrefab != null)
        {
            Debug.Log("<color=green>[PuzzleManager]</color> Wykryto rozwi¹zanie zagadki! Tworzê Canvas z prefabu...");

            // Instantiating (tworzenie kopii) prefabu na scenie w momencie wygranej
            GameObject spawnedCanvas = Instantiate(successCanvasPrefab);

            // Opcjonalnie: upewniamy siê, ¿e nowo stworzony obiekt na pewno jest w³¹czony
            spawnedCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("<color=red>[PuzzleManager] B£¥D:</color> Nie przypisano prefabu Canvasu w Inspektorze!");
        }
    }
}