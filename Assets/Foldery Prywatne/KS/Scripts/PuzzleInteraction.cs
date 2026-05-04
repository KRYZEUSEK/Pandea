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

    // Statyczna flaga - u¿yj jej w skrypcie Pauzy:
    // if (PuzzleInteraction.isPuzzleActive) return;
    public static bool isPuzzleActive = false;

    private void Start()
    {
        // Szukamy obiektu UI zaraz po uruchomieniu gry
        FindPuzzleUI();
    }

    private void FindPuzzleUI()
    {
        // Resources.FindObjectsOfTypeAll znajdzie obiekty nawet wy³¹czone (SetActive(false))
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Sprawdzamy nazwê i upewniamy siê, ¿e obiekt nale¿y do sceny (nie jest prefabem w Assets)
            if (obj.name == puzzleObjectName && obj.scene.name != null)
            {
                puzzleObject = obj;
                Debug.Log("<color=green>PuzzleInteraction:</color> Znaleziono i podpiêto UI: " + obj.name);
                return;
            }
        }
        Debug.LogError("<color=red>PuzzleInteraction B£¥D:</color> Nie znaleziono na scenie obiektu: " + puzzleObjectName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Pobieramy komponenty z gracza
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
        // 2. Zamykanie na klawisz Escape (obs³uguje tylko zagadkê)
        else if (isPuzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    public void OpenPuzzle()
    {
        if (puzzleObject == null) FindPuzzleUI();

        if (puzzleObject != null)
        {
            isPuzzleActive = true;
            puzzleObject.SetActive(true);

            // Blokowanie gracza
            if (playerAgent != null)
            {
                playerAgent.isStopped = true;
                playerAgent.enabled = false;
            }

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Odblokowanie kursora do rozwi¹zywania zagadki
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        // MECHANIZM RATUNKOWY: Jeœli referencje zniknê³y, szukamy gracza ponownie po Tagu
        if (playerAgent == null || playerController == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                if (playerAgent == null) playerAgent = player.GetComponent<NavMeshAgent>();
                if (playerController == null) playerController = player.GetComponent<PlayerControllerClick1>();
            }
        }

        // Odblokowanie ruchu
        if (playerAgent != null)
        {
            playerAgent.enabled = true; // Najpierw komponent
            playerAgent.isStopped = false; // Potem agent
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    /// <summary>
    /// Wywo³aj tê metodê, gdy gracz rozwi¹¿e zagadkê!
    /// </summary>
    public void CompletePuzzle()
    {
        Debug.Log("<color=cyan>PuzzleInteraction:</color> Zagadka rozwi¹zana!");

        // 1. Przywróæ ruch gracza
        ClosePuzzle();

        // 2. Zmieñ tag obiektu Triggera na Untagged (domyœlny brak tagu)
        this.gameObject.tag = "Untagged";

        // 3. Wy³¹cz skrypt i Collider, aby nie da³o siê go u¿yæ ponownie
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;

        this.enabled = false;
    }
}