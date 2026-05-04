using UnityEngine;
using UnityEngine.AI;

public class PuzzleInteraction : MonoBehaviour
{
    [Header("Ustawienia UI (Wpisz DOK£ADN¥ nazwê obiektu na scenie)")]
    public string puzzleObjectName = "PuzzleRoot";
    private GameObject puzzleObject;

    [Header("Referencje do Gracza (Pobierane automatycznie)")]
    private NavMeshAgent playerAgent;
    private PlayerControllerClick1 playerController;

    private bool isPlayerInRange = false;
    private bool isPuzzleActive = false;

    private void Start()
    {
        // Szukamy obiektu UI zaraz po uruchomieniu gry
        FindPuzzleUI();
    }

    private void FindPuzzleUI()
    {
        // Resources.FindObjectsOfTypeAll znajdzie obiekty, które s¹ wy³¹czone (SetActive(false))
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Sprawdzamy nazwê i upewniamy siê, ¿e obiekt nale¿y do sceny (nie jest prefabem w oknie Project)
            if (obj.name == puzzleObjectName && obj.scene.name != null)
            {
                puzzleObject = obj;
                Debug.Log("<color=green>PuzzleInteraction:</color> Znaleziono i podpiêto obiekt: " + obj.name);
                return;
            }
        }

        Debug.LogError("<color=red>PuzzleInteraction B£¥D:</color> Nie znaleziono na scenie obiektu o nazwie: " + puzzleObjectName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Pobieramy komponenty z gracza, który wszed³ w Trigger
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
        // 1. Otwieranie na klawisz F
        if (isPlayerInRange && !isPuzzleActive && Input.GetKeyDown(KeyCode.F))
        {
            OpenPuzzle();
        }
        // 2. Zamykanie na klawisz Escape
        else if (isPuzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    public void OpenPuzzle()
    {
        // Jeœli obiekt UI nie zosta³ znaleziony w Start(), spróbuj ponownie
        if (puzzleObject == null) FindPuzzleUI();

        if (puzzleObject != null)
        {
            isPuzzleActive = true;
            puzzleObject.SetActive(true);

            // BLOKOWANIE RUCHU
            if (playerAgent != null)
            {
                playerAgent.isStopped = true;
                playerAgent.enabled = false;
            }

            // BLOKOWANIE KONTROLERA (Inputu)
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Opcjonalnie: Odblokuj kursor myszy, jeœli Twoja gra go ukrywa
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        // ODBLOKOWANIE RUCHU
        if (playerAgent != null)
        {
            playerAgent.enabled = true;
            playerAgent.isStopped = false;
        }

        // ODBLOKOWANIE KONTROLERA
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Opcjonalnie: Ponowne zablokowanie kursora, jeœli jest to wymagane przez Twój kontroler
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
}