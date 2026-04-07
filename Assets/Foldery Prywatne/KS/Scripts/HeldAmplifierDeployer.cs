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

    [Header("Kara za b³¹d")]
    public float timePenalty = 10f;
    public float failCooldown = 2.0f;
    public GameObject explosionEffectPrefab;

    [Header("UI Kalibracji (Wyszukiwanie po nazwie)")]
    public string calibrationContainerName = "CalibrationCanvas";
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

    private GameObject warningUI;
    private GameObject calibrationUIContainer;
    private RectTransform pointerRect;
    private Coroutine warningCoroutine;

    private bool isCalibrating = false;
    private bool isOnCooldown = false;
    private float calibrationValue = 0f;
    private bool movingForward = true;

    // --- POPRAWKA 1: Sprz¹tanie po wyci¹gniêciu przedmiotu ---
    void OnEnable()
    {
        isCalibrating = false;
        isOnCooldown = false;
        calibrationValue = 0f;
        movingForward = true;
    }

    // --- POPRAWKA 2: Sprz¹tanie ratunkowe, gdy gracz chowa przedmiot ---
    void OnDisable()
    {
        // Wy³¹czamy UI kalibracji na twardo
        if (calibrationUIContainer != null)
            calibrationUIContainer.SetActive(false);

        // Wy³¹czamy napis ostrzegawczy na twardo
        if (warningUI != null)
            warningUI.SetActive(false);

        // Resetujemy stany, ¿eby nie zablokowaæ przedmiotu permanentnie
        isCalibrating = false;
        isOnCooldown = false;

        // Zatrzymujemy wszelkie timery
        StopAllCoroutines();
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
        }

        if (Input.GetKeyDown(deployKey) && !isOnCooldown)
        {
            HandleAction();
        }
    }

    void HandleAction()
    {
        FindUIElements();

        GameObject ship = GameObject.FindGameObjectWithTag(shipTag);
        if (ship != null && Vector3.Distance(transform.position, ship.transform.position) < minDistanceFromShip)
        {
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            warningCoroutine = StartCoroutine(ShowWarningTimer());
            if (isCalibrating) StopCalibration();
            return;
        }

        if (!isCalibrating)
        {
            StartCalibration();
        }
        else
        {
            CheckResult();
        }
    }

    void StartCalibration()
    {
        isCalibrating = true;
        calibrationValue = 0f;
        movingForward = true;

        if (calibrationUIContainer != null) calibrationUIContainer.SetActive(true);
    }

    void UpdatePointer()
    {
        if (movingForward)
        {
            calibrationValue += Time.deltaTime * calibrationSpeed;
            if (calibrationValue >= 1f) { calibrationValue = 1f; movingForward = false; }
        }
        else
        {
            calibrationValue -= Time.deltaTime * calibrationSpeed;
            if (calibrationValue <= 0f) { calibrationValue = 0f; movingForward = true; }
        }

        if (pointerRect != null)
        {
            pointerRect.anchorMin = new Vector2(calibrationValue, 0);
            pointerRect.anchorMax = new Vector2(calibrationValue, 1);
            pointerRect.anchoredPosition = Vector2.zero;
        }
    }

    void CheckResult()
    {
        if (calibrationValue >= sweetSpotMin && calibrationValue <= sweetSpotMax)
        {
            DeployAmplifier();
        }
        else
        {
            ApplyFailureConsequences();
        }
    }

    void ApplyFailureConsequences()
    {
        StopCalibration();
        StartCoroutine(CooldownRoutine());

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.ModifyTime(-timePenalty);
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(failCooldown);
        isOnCooldown = false;
    }

    void StopCalibration()
    {
        isCalibrating = false;
        calibrationValue = 0f;

        if (calibrationUIContainer != null) calibrationUIContainer.SetActive(false);
    }

    void FindUIElements()
    {
        if (warningUI != null && calibrationUIContainer != null && pointerRect != null) return;

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (warningUI == null && child.name == warningUIName)
                    warningUI = child.gameObject;

                if (calibrationUIContainer == null && child.name == calibrationContainerName)
                    calibrationUIContainer = child.gameObject;

                if (pointerRect == null && child.name == pointerName)
                    pointerRect = child.GetComponent<RectTransform>();
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