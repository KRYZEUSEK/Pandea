using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Collections;

public class GameOverHandler : MonoBehaviour
{
    [Header("Referencje (Znajdowane automatycznie)")]
    private Animator playerAnimator;
    private NavMeshAgent playerAgent;

    [Header("Ustawienia")]
    public string deathAnimationTrigger = "Die";
    public string sceneToLoad = "MainMenu";
    public float delayBeforeSceneLoad = 3f;

    private void Start()
    {
        // Nas≥uchujemy, kiedy TimeManager og≥osi koniec czasu
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeFinished += HandleGameOver;
        }

        // Szukamy gracza od razu na starcie
        FindPlayer();
    }

    private void OnDestroy()
    {
        // Odpinamy siÍ od eventu przy zniszczeniu obiektu
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeFinished -= HandleGameOver;
        }
    }

    // Nowa metoda do wyszukiwania gracza na scenie
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerAgent = player.GetComponent<NavMeshAgent>();
        }
    }

    private void HandleGameOver()
    {
        Debug.Log("<color=red>Koniec czasu! ZatrzymujÍ NavMeshAgenta.</color>");

        // MECHANIZM RATUNKOWY: Jeúli gracz zrespi≥ siÍ pÛüniej i referencje sπ puste, szukamy jeszcze raz!
        if (playerAgent == null || playerAnimator == null)
        {
            FindPlayer();
        }

        // --- WY£•CZANIE NAV MESH AGENTA ---
        if (playerAgent != null)
        {
            // 1. Jeúli agent aktualnie gdzieú idzie, kaøemy mu siÍ zatrzymaÊ
            if (playerAgent.isOnNavMesh)
            {
                playerAgent.isStopped = true;
                playerAgent.ResetPath(); // Dodatkowe czyszczenie úcieøki
            }

            // 2. Ca≥kowicie WY£•CZAMY komponent NavMeshAgent
            playerAgent.enabled = false;
        }
        else
        {
            Debug.LogWarning("GameOverHandler: Nie znaleziono NavMeshAgenta na graczu!");
        }

        // --- ODPALANIE ANIMACJI ---
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(deathAnimationTrigger);
        }
        else
        {
            Debug.LogWarning("GameOverHandler: Nie znaleziono Animatora na graczu!");
        }

        // --- ZMIANA SCENY PO OP”èNIENIU ---
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}