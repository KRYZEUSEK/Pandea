using UnityEngine;
using UnityEngine.AI;

public class PuzzleInteraction : MonoBehaviour
{
    [Header("Ustawienia UI (Wpisz DOK�ADN� nazw� obiektu na scenie)")]
    public string puzzleObjectName = "PuzzleRoot";
    private GameObject puzzleObject;

    [Header("Ustawienia Tekstu Interakcji")]
    [Tooltip("Nazwa obiektu z tekstem (np. 'Wci�nij F, aby u�o�y�').")]
    public string interactionTextName = "InteractionText";
    private GameObject interactionTextObject;

    [Header("Referencje do Gracza (Pobierane automatycznie)")]
    private NavMeshAgent playerAgent;
    private PlayerControllerClick1 playerController;

    private bool isPlayerInRange = false;

    // Statyczna flaga - uyj jej w skrypcie Pauzy i chodzenia
    public static bool isPuzzleActive = false;

    [Header("Opcjonalna Cutscenka (In-Scene Overlay)")]
    [Tooltip("Dokładna nazwa obiektu Canvasu cutscenki na scenie.")]
    public string cutsceneCanvasName;

    private GameObject FindGameObjectEvenInactive(string goName)
    {
        if (string.IsNullOrEmpty(goName)) return null;

        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.isLoaded)
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                if (rootObj.name == goName) return rootObj;

                Transform[] allChildren = rootObj.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == goName)
                    {
                        return child.gameObject;
                    }
                }
            }
        }
        return null;
    }

    private void Start()
    {
        FindPuzzleUI();
        FindInteractionText();
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
        Debug.LogError("<color=red>PuzzleInteraction BD:</color> Nie znaleziono na scenie obiektu: " + puzzleObjectName);
    }

    private void FindInteractionText()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == interactionTextName && obj.scene.name != null)
            {
                interactionTextObject = obj;
                interactionTextObject.SetActive(false); // Upewniamy si, e na start jest wyczony
                return;
            }
        }
        Debug.LogWarning("<color=yellow>PuzzleInteraction OSTRZEENIE:</color> Nie znaleziono obiektu tekstu: " + interactionTextName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (playerAgent == null) playerAgent = other.GetComponent<NavMeshAgent>();
            if (playerController == null) playerController = other.GetComponent<PlayerControllerClick1>();

            // Wczamy tekst interakcji, jeli zagadka nie jest wanie rozwizywana
            if (interactionTextObject != null && !isPuzzleActive)
            {
                interactionTextObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Wyczamy tekst, gdy gracz odejdzie
            if (interactionTextObject != null)
            {
                interactionTextObject.SetActive(false);
            }
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
        // Sprawdzamy tekst, w razie gdyby umkn
        if (interactionTextObject == null) FindInteractionText();

        if (puzzleObject != null)
        {
            isPuzzleActive = true;
            puzzleObject.SetActive(true);

            // Wyczamy tekst interakcji po wczeniu zagadki
            if (interactionTextObject != null)
            {
                interactionTextObject.SetActive(false);
            }

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
    /// Zwyke wyjcie z zagadki (np. klawiszem ESC lub przyciskiem "Wyjd")
    /// </summary>
    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        // Jeli gracz anulowa zagadk, ale dalej stoi w Triggerze - ponownie poka tekst
        if (interactionTextObject != null && isPlayerInRange)
        {
            interactionTextObject.SetActive(true);
        }

        RestorePlayerMovement();
    }

    /// <summary>
    /// Wywoywane przez PuzzleGridManager po pomylnym uoeniu!
    /// </summary>
    public void CompletePuzzle()
    {
        Debug.Log("<color=cyan>[PuzzleInteraction]</color> Zagadka rozwizana!");

        isPuzzleActive = false;
        if (puzzleObject != null) puzzleObject.SetActive(false);

        // Zabezpieczenie: trwale wyczamy tekst interakcji
        if (interactionTextObject != null) interactionTextObject.SetActive(false);

        // 1. Odblokuj gracza lub włącz cutscenkę
        GameObject cutsceneGo = FindGameObjectEvenInactive(cutsceneCanvasName);
        if (cutsceneGo != null)
        {
            cutsceneGo.SetActive(true);
            Debug.Log("<color=cyan>[PuzzleInteraction]</color> Uruchamianie cutscenki nakładkowej o nazwie: " + cutsceneCanvasName);
        }
        else
        {
            RestorePlayerMovement();
        }

        // 2. Zmie tag obiektu Triggera na Untagged (domylny brak tagu)
        this.gameObject.tag = "Untagged";

        // Powiadom QuestManager o ukonczeniu kroku
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteCurrentStep();
        }

        // 3. Wycz skrypt i Collider, aby nie dao si go uy ponownie
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;

        this.enabled = false;
    }

    /// <summary>
    /// OPCJA ATOMOWA: Znajduje prawdziwego gracza na scenie i siowo wcza mu komponenty
    /// </summary>
    private void RestorePlayerMovement()
    {
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");

        if (realPlayer != null)
        {
            NavMeshAgent realAgent = realPlayer.GetComponent<NavMeshAgent>();
            PlayerControllerClick1 realController = realPlayer.GetComponent<PlayerControllerClick1>();

            // W��czamy NavMeshAgent
            if (realAgent != null)
            {
                // Przyci�gamy do NavMesha, by unikn�� b��d�w
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

            // W��czamy PlayerController
            if (realController != null)
            {
                realController.enabled = true;
            }
        }
        else
        {
            Debug.LogError("<color=red>[B��D]</color> Skrypt pr�bowa� przywr�ci� ruch, ale nie znalaz� gracza z tagiem 'Player' na scenie!");
        }
    }
}