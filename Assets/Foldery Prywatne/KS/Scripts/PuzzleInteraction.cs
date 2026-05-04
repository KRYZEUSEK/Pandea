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

    // Statyczna flaga - u¿yj jej w skrypcie Pauzy i chodzenia
    public static bool isPuzzleActive = false;

    private void Start()
    {
        FindPuzzleUI();
    }

    private void FindPuzzleUI()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == puzzleObjectName && obj.scene.name != null)
            {
                puzzleObject = obj;
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
        // 2. Anulowanie/Zamykanie na klawisz Escape
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

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Zwyk³e wyjœcie z zagadki (np. klawiszem ESC lub przyciskiem "WyjdŸ")
    /// </summary>
    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        RestorePlayerMovement();
    }

    /// <summary>
    /// Wywo³ywane przez PuzzleGridManager po pomyœlnym u³o¿eniu!
    /// </summary>
    public void CompletePuzzle()
    {
        Debug.Log("<color=cyan>[PuzzleInteraction]</color> Zagadka rozwi¹zana! Trwale odblokowujê gracza...");

        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        // 1. Odblokuj gracza
        RestorePlayerMovement();

        // 2. Zmieñ tag obiektu Triggera na Untagged (domyœlny brak tagu)
        this.gameObject.tag = "Untagged";

        // 3. Wy³¹cz skrypt i Collider, aby nie da³o siê go u¿yæ ponownie
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;

        this.enabled = false;
    }

    /// <summary>
    /// OPCJA ATOMOWA: Znajduje prawdziwego gracza na scenie i si³owo w³¹cza mu komponenty
    /// </summary>
    private void RestorePlayerMovement()
    {
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");

        if (realPlayer != null)
        {
            NavMeshAgent realAgent = realPlayer.GetComponent<NavMeshAgent>();
            PlayerControllerClick1 realController = realPlayer.GetComponent<PlayerControllerClick1>();

            // W³¹czamy NavMeshAgent
            if (realAgent != null)
            {
                // Przyci¹gamy do NavMesha, by unikn¹æ b³êdów
                NavMeshHit hit;
                if (NavMesh.SamplePosition(realPlayer.transform.position, out hit, 3.0f, NavMesh.AllAreas))
                {
                    realPlayer.transform.position = hit.position;
                }

                realAgent.enabled = true;
                realAgent.isStopped = false;

                if (realAgent.isOnNavMesh)
                {
                    realAgent.ResetPath();
                }
            }

            // W³¹czamy PlayerController
            if (realController != null)
            {
                realController.enabled = true;
            }
        }
        else
        {
            Debug.LogError("<color=red>[B£¥D]</color> Skrypt próbowa³ przywróciæ ruch, ale nie znalaz³ gracza z tagiem 'Player' na scenie!");
        }
    }
}