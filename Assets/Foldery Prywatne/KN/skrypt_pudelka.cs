using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class skrypt_pudelka : MonoBehaviour
{
    // --- GLOBALNA FLAGA MUTEXU DLA WZMACNIACZA ---
    public static bool isAnyPudelkoActive = false;

    // --- STRUKTURA DLA AKTYWNEGO PUDElKA ---
    public static skrypt_pudelka activePudelko;

    [Header("UI Zagadki")]
    [Tooltip("Dokladna nazwa obiektu Canvasu zagadki na scenie (uzywana jako fallback).")]
    public string puzzleCanvasName = "PuzzleCanvas";
    private GameObject puzzleCanvas;

    [Header("Ustawienia Sceny Lore")]
    [Tooltip("Nazwa sceny lore/cutscenki do wczytania po rozwiazaniu tej konkretnej skrzynki. Pozostaw puste, aby wylaczyc wczytywanie sceny.")]
    public string sceneToLoad = "";

    [Header("Referencje do Gracza (Pobierane automatycznie)")]
    private NavMeshAgent playerAgent;
    private PlayerControllerClick1 playerController;

    private bool isPlayerNear = false;

    private void Start()
    {
        FindPuzzleCanvas();
    }

    private void FindPuzzleCanvas()
    {
        if (puzzleCanvas != null) return;

        // 1. Pancerny i bezbledny zapis wyszukiwania w aktywnej scenie (wspiera nieaktywne dzieci w hierarchii)
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.isLoaded)
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                skrypt_canvas canvasScript = rootObj.GetComponentInChildren<skrypt_canvas>(true);
                if (canvasScript != null)
                {
                    puzzleCanvas = canvasScript.gameObject;
                    puzzleCanvas.SetActive(false); // Upewniamy sie, ze na start jest wylaczony
                    return;
                }
            }
        }

        // 2. Fallback: szukanie po komponencie w zaladowanej scenie (scene.IsValid)
        skrypt_canvas[] canvases = Resources.FindObjectsOfTypeAll<skrypt_canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas.gameObject.scene.IsValid())
            {
                puzzleCanvas = canvas.gameObject;
                puzzleCanvas.SetActive(false);
                return;
            }
        }

        // 3. Fallback: szukanie po nazwie w zaladowanej scenie
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if ((obj.name == puzzleCanvasName || obj.name == "Canvas" || obj.name == "ZagadkaKNcanvas") && obj.gameObject.scene.IsValid())
            {
                puzzleCanvas = obj;
                puzzleCanvas.SetActive(false);
                return;
            }
        }
        Debug.LogWarning("skrypt_pudelka: Nie znaleziono obiektu Canvasu zagadki w scenie!");
    }

    void Update()
    {
        // 0. Wykrywanie zewnetrznego zamkniecia Canvasu (np. przyciskiem "Wyjdz" w UI)
        if (isAnyPudelkoActive && puzzleCanvas != null && !puzzleCanvas.activeSelf)
        {
            ClosePuzzleUI();
            return;
        }

        // 1. Otwieranie na klawisz F (blokowane, gdy trwa kalibracja wzmacniacza)
        if (isPlayerNear && !isAnyPudelkoActive && !HeldAmplifierDeployer.IsAnyCalibrating && Input.GetKeyDown(KeyCode.F))
        {
            if (puzzleCanvas == null) FindPuzzleCanvas();

            if (puzzleCanvas != null && !puzzleCanvas.activeSelf)
            {
                OpenPuzzleUI();
            }
        }
        // 2. Anulowanie/Zamykanie na klawisz Escape
        else if (isAnyPudelkoActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzleUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (playerAgent == null) playerAgent = other.GetComponent<NavMeshAgent>();
            if (playerController == null) playerController = other.GetComponent<PlayerControllerClick1>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            // Zamykamy UI, jesli gracz odejdzie za daleko
            if (isAnyPudelkoActive)
            {
                ClosePuzzleUI();
            }
        }
    }

    private void OpenPuzzleUI()
    {
        puzzleCanvas.SetActive(true);
        isAnyPudelkoActive = true;
        activePudelko = this;
        
        // Blokowanie poruszania sie gracza
        if (playerAgent != null)
        {
            playerAgent.isStopped = true;
            playerAgent.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Odblokowujemy kursor, zeby gracz mogl klikac po UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePuzzleUI()
    {
        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);
        isAnyPudelkoActive = false;

        // Odblokowujemy ruch gracza
        RestorePlayerMovement();

        if (activePudelko == this)
        {
            activePudelko = null;
        }

        // Jesli skrzynka zostala rozwiazana (ma usuniety tag), wylaczamy skrypt i collider
        if (gameObject.tag == "Untagged")
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
            this.enabled = false;
        }
    }

    private void RestorePlayerMovement()
    {
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");

        if (realPlayer != null)
        {
            NavMeshAgent realAgent = realPlayer.GetComponent<NavMeshAgent>();
            PlayerControllerClick1 realController = realPlayer.GetComponent<PlayerControllerClick1>();

            // Wlaczamy NavMeshAgent
            if (realAgent != null)
            {
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

            // Wlaczamy PlayerController
            if (realController != null)
            {
                realController.enabled = true;
            }
        }
    }
}
