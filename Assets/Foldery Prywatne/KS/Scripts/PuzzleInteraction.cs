using UnityEngine;
using UnityEngine.AI;

public class PuzzleInteraction : MonoBehaviour
{
    [Header("Ustawienia UI")]
    public GameObject puzzleObject;

    [Header("Referencje do Gracza")]
    public NavMeshAgent playerAgent;
    public PlayerControllerClick1 playerController; // Przeci¹gnij tutaj skrypt kontrolera gracza

    private bool isPlayerInRange = false;
    private bool isPuzzleActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Automatyczne pobieranie referencji, jeœli nie s¹ przypisane w inspektorze
            if (playerAgent == null) playerAgent = other.GetComponent<NavMeshAgent>();
            if (playerController == null) playerController = other.GetComponent<PlayerControllerClick1>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void Update()
    {
        // 1. Otwieranie na F
        if (isPlayerInRange && !isPuzzleActive && Input.GetKeyDown(KeyCode.F))
        {
            OpenPuzzle();
        }
        // 2. Zamykanie na Escape
        else if (isPuzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    public void OpenPuzzle()
    {
        isPuzzleActive = true;
        if (puzzleObject != null) puzzleObject.SetActive(true);

        // BLOKOWANIE RUCHU I SKOKU
        // Wy³¹czamy agenta, ¿eby przesta³ wyznaczaæ trasê
        if (playerAgent != null)
        {
            playerAgent.isStopped = true;
            playerAgent.enabled = false;
        }

        // Wy³¹czamy skrypt kontrolera, co dziêki Twojemu OnDisable() wy³¹cza Input System (skok/ruch)
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        // ODBLOKOWANIE RUCHU I SKOKU
        if (playerAgent != null)
        {
            playerAgent.enabled = true;
            playerAgent.isStopped = false;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}