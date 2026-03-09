using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI; // <--- BARDZO WAØNE: To pozwala na uøywanie NavMeshAgent
using System.Collections;

public class GameOverHandler : MonoBehaviour
{
    [Header("Referencje")]
    public Animator playerAnimator;
    public NavMeshAgent playerAgent; // <--- Pole na Twojego NavMeshAgenta

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
    }

    private void OnDestroy()
    {
        // Odpinamy siÍ od eventu przy zniszczeniu obiektu (dobra praktyka)
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeFinished -= HandleGameOver;
        }
    }

    private void HandleGameOver()
    {
        Debug.Log("<color=red>Koniec czasu! ZatrzymujÍ NavMeshAgenta.</color>");

        // --- WY£•CZANIE NAV MESH AGENTA ---
        if (playerAgent != null)
        {
            // 1. Jeúli agent aktualnie gdzieú idzie, kaøemy mu siÍ zatrzymaÊ
            if (playerAgent.isOnNavMesh)
            {
                playerAgent.isStopped = true;
            }

            // 2. Ca≥kowicie WY£•CZAMY komponent NavMeshAgent
            playerAgent.enabled = false;
        }

        // --- ODPALANIE ANIMACJI ---
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(deathAnimationTrigger);
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