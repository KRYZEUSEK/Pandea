using UnityEngine;
using System.Collections;

public class HeldAmplifierDeployer : MonoBehaviour
{
    [Header("Ustawienia Interakcji")]
    public KeyCode deployKey = KeyCode.F;
    public float requiredHoldTime = 2.0f;

    [Header("Ustawienia Bezpieczeñstwa")]
    public string requiredParentName = "ToolHoldPoint";

    [Header("Ustawienia Spawnu")]
    public GameObject amplifierPrefab;

    [Header("Ustawienia Zasiêgu (Statek)")]
    public string shipTag = "Ship";
    public float minDistanceFromShip = 20f;

    [Tooltip("Dok³adna nazwa obiektu tekstu w hierarchii. Skrypt sam go znajdzie!")]
    public string warningUIName = "TooCloseToBaseText"; // <-- ZMIANA: Szukamy po nazwie

    private float currentHoldTime = 0f;
    private GameObject shipObject;
    private GameObject warningUI; // Skrypt sam wype³ni tê zmienn¹
    private Coroutine warningCoroutine;

    void Update()
    {
        if (transform.parent == null || transform.parent.name != requiredParentName)
        {
            currentHoldTime = 0f;
            return;
        }

        // --- 2. REAKCJA NA SAM KLIK ---
        if (Input.GetKeyDown(deployKey))
        {
            FindWarningUI(); // Szukamy napisu w UI

            if (shipObject == null) shipObject = GameObject.FindGameObjectWithTag(shipTag);

            if (shipObject != null)
            {
                float distanceToShip = Vector3.Distance(transform.position, shipObject.transform.position);

                if (distanceToShip < minDistanceFromShip)
                {
                    if (warningCoroutine != null) StopCoroutine(warningCoroutine);
                    warningCoroutine = StartCoroutine(ShowWarningTimer());
                }
            }
        }

        // --- 3. OBS£UGA PRZYTRZYMANIA KLAWISZA ---
        if (Input.GetKey(deployKey))
        {
            if (shipObject != null && Vector3.Distance(transform.position, shipObject.transform.position) < minDistanceFromShip)
            {
                currentHoldTime = 0f;
                return;
            }

            if (warningUI != null && warningUI.activeSelf)
            {
                if (warningCoroutine != null) StopCoroutine(warningCoroutine);
                warningUI.SetActive(false);
            }

            currentHoldTime += Time.deltaTime;

            if (currentHoldTime >= requiredHoldTime)
            {
                DeployAmplifier();
                currentHoldTime = 0f;
            }
        }
        else
        {
            if (currentHoldTime > 0) currentHoldTime = 0f;
        }
    }

    // --- NOWA FUNKCJA: Inteligentne szukanie wy³¹czonego UI ---
    void FindWarningUI()
    {
        if (warningUI != null) return; // Jeœli ju¿ znalaz³, nie szukaj ponownie

        // Szuka wszystkich Canvasów i ich dzieci (nawet tych z odznaczonym ptaszkiem)
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == warningUIName)
                {
                    warningUI = child.gameObject;
                    Debug.Log("Sukces: Skrypt znalaz³ i podpi¹³ napis: " + warningUIName);
                    return;
                }
            }
        }
        Debug.LogError("B³¹d: Nie znaleziono obiektu UI o nazwie: " + warningUIName);
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
        if (warningUI != null) warningUI.SetActive(false);

        if (amplifierPrefab == null) return;

        Vector3 spawnPosition = transform.root.position;
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