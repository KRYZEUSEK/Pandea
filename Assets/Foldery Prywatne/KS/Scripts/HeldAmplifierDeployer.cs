using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.AI;

public class HeldAmplifierDeployer : MonoBehaviour
{
    [Header("Ustawienia Interakcji (3 Etapy)")]
    public KeyCode deployKey = KeyCode.F;
    public int requiredStages = 3;
    public float baseCalibrationSpeed = 1.2f;
    public float speedMultiplierPerStage = 1.25f;
    public float sweetSpotWidth = 0.15f;

    [Header("Ustawienia Ruchomej Strefy (Fina³)")]
    public float sweetSpotMoveSpeed = 0.8f;

    [Header("Kara za b³¹d")]
    public float timePenalty = 10f;
    [Tooltip("Czas blokady po b³êdzie (sekundy)")]
    public float failCooldown = 2.0f;
    public GameObject explosionEffectPrefab;

    [Header("UI Kalibracji (Nazwy)")]
    public string calibrationContainerName = "CalibrationCanvas";
    public string pointerName = "Pointer";
    public string sweetSpotName = "SweetSpot";

    [Header("Ustawienia Bezpieczeñstwa / Spawnu")]
    public string requiredParentName = "ToolHoldPoint";
    public GameObject amplifierPrefab;
    public float wysokoscSpawnuOffset = -1.0f;

    [Header("Ustawienia Zasiêgu")]
    public string shipTag = "Ship";
    public float minDistanceFromShip = 20f;
    public string warningUIName = "TooCloseToBaseText";

    // Prywatne UI
    private GameObject warningUI;
    private GameObject calibrationUIContainer;
    private RectTransform pointerRect;
    private Image pointerImage;
    private RectTransform sweetSpotRect;
    private Coroutine warningCoroutine;

    private NavMeshAgent playerAgent;

    // Logika stanu
    private bool isCalibrating = false;
    private float pointerValue = 0f;
    private bool pointerMovingForward = true;

    private int currentStage = 1;
    private float currentPointerSpeed;
    private float sweetSpotMin;
    private float sweetSpotMax;

    private float sweetSpotCenter = 0.5f;
    private bool sweetSpotMovingForward = true;

    // --- POPRAWKA 1: Bezpieczny system blokady (odporny na exploity) ---
    private float cooldownEndTime = 0f;

    void OnEnable()
    {
        if (playerAgent == null)
        {
            playerAgent = transform.root.GetComponentInChildren<NavMeshAgent>();
        }
        // Upewniamy siê, ¿e po wyci¹gniêciu przedmiotu stan kalibracji jest czysty
        ResetCalibrationVariables();
    }

    void OnDisable()
    {
        StopCalibration();
        StopAllCoroutines();
    }

    void ResetCalibrationVariables()
    {
        isCalibrating = false;
        pointerValue = 0f;
        pointerMovingForward = true;
        currentStage = 1;
    }

    void Update()
    {
        if (transform.parent == null || transform.parent.name != requiredParentName)
        {
            if (isCalibrating) StopCalibration();
            return;
        }

        if (isCalibrating)
        {
            UpdatePointer();

            if (currentStage == requiredStages)
            {
                UpdateMovingSweetSpot();
            }
        }

        // Sprawdzamy czy blokada czasowa ju¿ minê³a
        bool isOnCooldown = Time.time < cooldownEndTime;

        if (Input.GetKeyDown(deployKey) && !isOnCooldown)
        {
            HandleAction();
        }
    }

    void HandleAction()
    {
        FindUIElements();

        if (calibrationUIContainer == null || pointerRect == null || sweetSpotRect == null)
        {
            Debug.LogWarning("Brak elementów UI Kalibracji na scenie! Rozk³adanie anulowane.");
            return;
        }

        GameObject ship = GameObject.FindGameObjectWithTag(shipTag);
        if (ship != null && Vector3.Distance(transform.position, ship.transform.position) < minDistanceFromShip)
        {
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            warningCoroutine = StartCoroutine(ShowWarningTimer());
            if (isCalibrating) StopCalibration();
            return;
        }

        if (!isCalibrating) StartCalibration();
        else CheckResult();
    }

    void StartCalibration()
    {
        ResetCalibrationVariables();
        isCalibrating = true;
        currentPointerSpeed = baseCalibrationSpeed;

        // --- POPRAWKA 2: Blokujemy ruch raz, na samym pocz¹tku ---
        if (playerAgent != null && playerAgent.isOnNavMesh)
        {
            playerAgent.isStopped = true;
            playerAgent.velocity = Vector3.zero;
        }

        RandomizeSweetSpot();
        if (calibrationUIContainer != null) calibrationUIContainer.SetActive(true);
    }

    void UpdateMovingSweetSpot()
    {
        float limit = sweetSpotWidth / 2f;

        if (sweetSpotMovingForward)
        {
            sweetSpotCenter += Time.deltaTime * sweetSpotMoveSpeed;
            if (sweetSpotCenter >= 1f - limit)
            {
                sweetSpotCenter = 1f - limit;
                sweetSpotMovingForward = false;
            }
        }
        else
        {
            sweetSpotCenter -= Time.deltaTime * sweetSpotMoveSpeed;
            if (sweetSpotCenter <= limit)
            {
                sweetSpotCenter = limit;
                sweetSpotMovingForward = true;
            }
        }

        sweetSpotMin = sweetSpotCenter - limit;
        sweetSpotMax = sweetSpotCenter + limit;

        UpdateSweetSpotUI();
    }

    void RandomizeSweetSpot()
    {
        float limit = sweetSpotWidth / 2f;
        sweetSpotCenter = Random.Range(limit, 1f - limit);

        sweetSpotMin = sweetSpotCenter - limit;
        sweetSpotMax = sweetSpotCenter + limit;
        UpdateSweetSpotUI();
    }

    void UpdateSweetSpotUI()
    {
        if (sweetSpotRect != null)
        {
            sweetSpotRect.anchorMin = new Vector2(sweetSpotMin, 0);
            sweetSpotRect.anchorMax = new Vector2(sweetSpotMax, 1);
            sweetSpotRect.anchoredPosition = Vector2.zero;
        }
    }

    void UpdatePointer()
    {
        if (pointerMovingForward)
        {
            pointerValue += Time.deltaTime * currentPointerSpeed;
            if (pointerValue >= 1f) { pointerValue = 1f; pointerMovingForward = false; }
        }
        else
        {
            pointerValue -= Time.deltaTime * currentPointerSpeed;
            if (pointerValue <= 0f) { pointerValue = 0f; pointerMovingForward = true; }
        }

        if (pointerRect != null)
        {
            pointerRect.anchorMin = new Vector2(pointerValue, 0);
            pointerRect.anchorMax = new Vector2(pointerValue, 1);
            pointerRect.anchoredPosition = Vector2.zero;
        }

        if (pointerImage != null)
        {
            pointerImage.color = (pointerValue >= sweetSpotMin && pointerValue <= sweetSpotMax) ? Color.green : Color.cyan;
        }
    }

    void CheckResult()
    {
        if (pointerValue >= sweetSpotMin && pointerValue <= sweetSpotMax)
        {
            if (currentStage >= requiredStages) DeployAmplifier();
            else
            {
                currentStage++;
                currentPointerSpeed *= speedMultiplierPerStage;
                // --- POPRAWKA 3: Zawsze zmieniaj miejsce strefy na nowym etapie ---
                RandomizeSweetSpot();
            }
        }
        else ApplyFailureConsequences();
    }

    void ApplyFailureConsequences()
    {
        StopCalibration();

        // Zapisujemy w czasie rzeczywistym, kiedy minie blokada
        cooldownEndTime = Time.time + failCooldown;
        Debug.Log("<color=orange>Wzmacniacz zablokowany - trwa restart systemów po spiêciu...</color>");

        if (TimeManager.Instance) TimeManager.Instance.ModifyTime(-timePenalty);
        if (explosionEffectPrefab) Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
    }

    void StopCalibration()
    {
        ResetCalibrationVariables();
        if (calibrationUIContainer != null) calibrationUIContainer.SetActive(false);

        // --- POPRAWKA 2: Bezpieczne odblokowanie ruchu ---
        if (playerAgent != null && playerAgent.isOnNavMesh)
        {
            playerAgent.ResetPath(); // Najpierw czyœcimy kolejkê klikniêæ gracza z czasu minigry!
            playerAgent.isStopped = false; // Potem go uwalniamy
        }
    }

    void FindUIElements()
    {
        if (warningUI != null && calibrationUIContainer != null && pointerRect != null && sweetSpotRect != null) return;

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == warningUIName) warningUI = child.gameObject;
                if (child.name == calibrationContainerName) calibrationUIContainer = child.gameObject;
                if (child.name == pointerName)
                {
                    pointerRect = child.GetComponent<RectTransform>();
                    pointerImage = child.GetComponent<Image>();
                }
                if (child.name == sweetSpotName) sweetSpotRect = child.GetComponent<RectTransform>();
            }
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
        StopCalibration(); // To odblokuje agenta gracza
        if (amplifierPrefab == null) return;

        Vector3 spawnPos = transform.root.position + new Vector3(0, wysokoscSpawnuOffset, 0);
        GameObject deployed = Instantiate(amplifierPrefab, spawnPos, transform.root.rotation);

        if (deployed.TryGetComponent<AmplifierTracker>(out var tracker)) tracker.Deploy();

        HotbarSelector hotbar = transform.root.GetComponentInChildren<HotbarSelector>();
        if (hotbar != null && hotbar.inventory != null && hotbar.inventory.Slots.Length > hotbar.CurrentIndex)
        {
            var activeSlot = hotbar.inventory.Slots[hotbar.CurrentIndex];
            if (activeSlot != null && activeSlot.item != null)
            {
                hotbar.inventory.RemoveItem(activeSlot.item, 1);
            }
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}