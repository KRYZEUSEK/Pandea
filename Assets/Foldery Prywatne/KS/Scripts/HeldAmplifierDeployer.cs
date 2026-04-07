using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HeldAmplifierDeployer : MonoBehaviour
{
    [Header("Ustawienia Interakcji (Kalibracja)")]
    public KeyCode deployKey = KeyCode.F;
    public float calibrationSpeed = 1.5f;
    public float sweetSpotMin = 0.70f;
    public float sweetSpotMax = 0.85f;

    [Header("UI Kalibracji (Wyszukiwanie po nazwie)")]
    [Tooltip("Dok³adna nazwa obiektu, który ma byæ w³¹czany/wy³¹czany (np. CalibrationCanvas)")]
    public string calibrationContainerName = "CalibrationCanvas";
    [Tooltip("Dok³adna nazwa obiektu wskaŸnika, który ma siê poruszaæ")]
    public string pointerName = "Pointer";

    [Header("Ustawienia Bezpieczeñstwa")]
    public string requiredParentName = "ToolHoldPoint";

    [Header("Ustawienia Spawnu")]
    public GameObject amplifierPrefab;
    public float wysokoscSpawnuOffset = -1.0f;

    [Header("Ustawienia Zasiêgu (Statek)")]
    public string shipTag = "Ship";
    public float minDistanceFromShip = 20f;
    public string warningUIName = "TooCloseToBaseText";

    private GameObject shipObject;

    // Zmienne UI (prywatne, skrypt znajdzie je sam)
    private GameObject warningUI;
    private GameObject calibrationUIContainer;
    private RectTransform pointerRect;

    private Coroutine warningCoroutine;

    private bool isCalibrating = false;
    private float calibrationValue = 0f;
    private bool movingForward = true;

    void Update()
    {
        if (transform.parent == null || transform.parent.name != requiredParentName)
        {
            StopCalibration();
            return;
        }

        if (isCalibrating)
        {
            // Ruch paska w górê i w dó³ (Ping-Pong)
            if (movingForward)
            {
                calibrationValue += Time.deltaTime * calibrationSpeed;
                if (calibrationValue >= 1f)
                {
                    calibrationValue = 1f;
                    movingForward = false;
                }
            }
            else
            {
                calibrationValue -= Time.deltaTime * calibrationSpeed;
                if (calibrationValue <= 0f)
                {
                    calibrationValue = 0f;
                    movingForward = true;
                }
            }

            // Aktualizacja pozycji wskaŸnika w UI
            if (pointerRect != null)
            {
                pointerRect.anchorMin = new Vector2(calibrationValue, 0);
                pointerRect.anchorMax = new Vector2(calibrationValue, 1);
                pointerRect.anchoredPosition = Vector2.zero;
            }
        }

        if (Input.GetKeyDown(deployKey))
        {
            // Szukamy ca³ego UI po nazwach (tylko raz, jeœli czegoœ brakuje)
            FindUIElements();

            if (shipObject == null) shipObject = GameObject.FindGameObjectWithTag(shipTag);

            if (shipObject != null && Vector3.Distance(transform.position, shipObject.transform.position) < minDistanceFromShip)
            {
                if (warningCoroutine != null) StopCoroutine(warningCoroutine);
                warningCoroutine = StartCoroutine(ShowWarningTimer());
                StopCalibration();
                return;
            }

            if (!isCalibrating)
            {
                // ROZPOCZÊCIE KALIBRACJI
                isCalibrating = true;
                calibrationValue = 0f;
                movingForward = true;

                if (calibrationUIContainer != null) calibrationUIContainer.SetActive(true);
            }
            else
            {
                // ZATWIERDZENIE KALIBRACJI
                if (calibrationValue >= sweetSpotMin && calibrationValue <= sweetSpotMax)
                {
                    DeployAmplifier();
                }
                else
                {
                    StopCalibration();
                }
            }
        }
    }

    void StopCalibration()
    {
        isCalibrating = false;
        calibrationValue = 0f;

        if (calibrationUIContainer != null) calibrationUIContainer.SetActive(false);
    }

    // --- ZAKTUALIZOWANA FUNKCJA: Szuka wszystkich potrzebnych elementów UI po nazwach ---
    void FindUIElements()
    {
        // Jeœli wszystko ju¿ mamy, nie marnujemy zasobów procesora na szukanie
        if (warningUI != null && calibrationUIContainer != null && pointerRect != null) return;

        // Szukamy we wszystkich Canvasach (nawet tych wy³¹czonych w hierarchii)
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (warningUI == null && child.name == warningUIName)
                {
                    warningUI = child.gameObject;
                }

                if (calibrationUIContainer == null && child.name == calibrationContainerName)
                {
                    calibrationUIContainer = child.gameObject;
                }

                if (pointerRect == null && child.name == pointerName)
                {
                    pointerRect = child.GetComponent<RectTransform>();
                }
            }
        }

        if (calibrationUIContainer == null || pointerRect == null)
        {
            Debug.LogWarning("Nie znaleziono UI Kalibracji! Upewnij siê, ¿e nazwy w Inspektorze zgadzaj¹ siê z nazwami w Hierarchii.");
        }
    }

    private IEnumerator ShowWarningTimer()
    {
        if (warningUI != null)
        {
            warningUI.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            warningUI.SetActive(false);
        }
    }

    void DeployAmplifier()
    {
        StopCalibration();
        if (warningUI != null) warningUI.SetActive(false);
        if (amplifierPrefab == null) return;

        Vector3 spawnPosition = transform.root.position + new Vector3(0, wysokoscSpawnuOffset, 0);
        GameObject deployedAmplifier = Instantiate(amplifierPrefab, spawnPosition, transform.root.rotation);

        AmplifierTracker tracker = deployedAmplifier.GetComponent<AmplifierTracker>();
        if (tracker != null) tracker.Deploy();

        HotbarSelector hotbar = transform.root.GetComponentInChildren<HotbarSelector>();
        if (hotbar != null && hotbar.inventory != null)
        {
            int activeIndex = hotbar.CurrentIndex;
            var activeSlot = hotbar.inventory.Slots[activeIndex];

            if (activeSlot != null && activeSlot.item != null)
            {
                hotbar.inventory.RemoveItem(activeSlot.item, 1);
            }
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}