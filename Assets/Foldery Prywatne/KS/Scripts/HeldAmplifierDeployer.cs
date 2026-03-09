using UnityEngine;

public class HeldAmplifierDeployer : MonoBehaviour
{
    [Header("Ustawienia Interakcji")]
    public KeyCode deployKey = KeyCode.F;
    public float requiredHoldTime = 2.0f;

    [Header("Ustawienia Bezpieczeñstwa")]
    public string requiredParentName = "ToolHoldPoint";

    [Header("Ustawienia Spawnu")]
    [Tooltip("Gotowy prefab wzmacniacza (z trackerem), który pojawi siê na ziemi.")]
    public GameObject amplifierPrefab;

    private float currentHoldTime = 0f;

    void Update()
    {
        // 1. Zabezpieczenie: Sprawdzamy, czy przedmiot na pewno jest w rêce gracza
        if (transform.parent == null || transform.parent.name != requiredParentName)
        {
            currentHoldTime = 0f;
            return;
        }

        // 2. Obs³uga przytrzymania klawisza F
        if (Input.GetKey(deployKey))
        {
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

    void DeployAmplifier()
    {
        if (amplifierPrefab == null)
        {
            Debug.LogError("Nie przypisano prefabu w polu Amplifier Prefab!");
            return;
        }

        // --- 1. ROZSTAWIANIE WZMACNIACZA NA ZIEMI ---
        Vector3 spawnPosition = transform.root.position;
        GameObject deployedAmplifier = Instantiate(amplifierPrefab, spawnPosition, transform.root.rotation);

        AmplifierTracker tracker = deployedAmplifier.GetComponent<AmplifierTracker>();
        if (tracker != null) tracker.Deploy();

        // --- 2. INTELIGENTNE USUWANIE Z EKWIPUNKU ---
        // Szukamy Twojego skryptu HotbarSelector na g³ównym obiekcie gracza
        HotbarSelector hotbar = transform.root.GetComponentInChildren<HotbarSelector>();

        if (hotbar != null && hotbar.inventory != null)
        {
            // Pobieramy numer slotu, który gracz ma teraz wybrany
            int activeIndex = hotbar.CurrentIndex;
            var activeSlot = hotbar.inventory.Slots[activeIndex];

            // Jeœli w tym slocie jest przedmiot, usuwamy z niego 1 sztukê
            if (activeSlot != null && activeSlot.item != null)
            {
                Debug.Log($"Usuwam {activeSlot.item.name} ze slotu nr {activeIndex}");
                hotbar.inventory.RemoveItem(activeSlot.item, 1);
            }
        }
        else
        {
            Debug.LogError("B£¥D: Nie mog³em znaleŸæ HotbarSelector na graczu!");
        }

        // --- 3. UKRYCIE WIZUALNE ---
        // Natychmiast ukrywamy obiekt, a system ekwipunku w u³amku sekundy sam 
        // usunie go komend¹ ClearHeldTool() z PlayerInteraction1
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}