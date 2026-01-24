using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // U¿ywamy do .OrderBy() i .FirstOrDefault()

public class PlayerInteraction : MonoBehaviour
{
    [Header("Setup (Required)")]
    [Tooltip("Obiekt 'HandSocket' (dziecko koœci rêki), do którego podpinamy narzêdzia.")]
    [SerializeField] private Transform handSocket;

    [Tooltip("G³ówny Collider gracza (np. CapsuleCollider). Potrzebny do ignorowania kolizji z trzymanym narzêdziem.")]
    [SerializeField] private Collider playerCollider;

    [Header("UI (Optional)")]
    [Tooltip("Tekst 'Naciœnij E, aby podnieœæ', który bêdzie pokazywany/ukrywany.")]
    [SerializeField] private GameObject pickupTextUI;

    // --- State ---
    private GameObject heldTool = null; // Narzêdzie, które aktualnie trzymamy
    private List<GameObject> nearbyTools = new List<GameObject>(); // Lista narzêdzi w zasiêgu
    private GameObject closestTool = null; // Najbli¿sze narzêdzie (cel interakcji)
    private bool isDropping = false; // Flaga do DropCooldown

    void Start()
    {
        // Ukryj UI na starcie
        if (pickupTextUI != null)
            pickupTextUI.SetActive(false);

        // Zabezpieczenie na wypadek zapomnienia o colliderze
        if (playerCollider == null)
        {
            Debug.LogWarning("Player Collider nie jest przypisany w PlayerInteraction. Próbujê pobraæ automatycznie.");
            playerCollider = GetComponentInParent<Collider>();
            if (playerCollider == null)
                Debug.LogError("Nie znaleziono Player Collider! Interakcje mog¹ nie dzia³aæ poprawnie.");
        }
    }

    void Update()
    {
        // SprawdŸ, czy trzymany przedmiot nie zosta³ zniszczony przez inny skrypt (np. FixableScript)
        if (heldTool != null && heldTool.gameObject == null)
        {
            heldTool = null; // Wyczyœæ stan, aby unikn¹æ b³êdów
        }

        // 1. Zawsze aktualizuj najbli¿sze narzêdzie i UI
        UpdateClosestTool();

        // 2. SprawdŸ, czy gracz chce podnieœæ/upuœciæ przedmiot
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Jeœli nic nie trzymamy i jesteœmy blisko narzêdzia
            if (heldTool == null && closestTool != null)
            {
                PickUpTool(closestTool);
            }
            // Jeœli coœ trzymamy
            else if (heldTool != null)
            {
                DropTool();
            }
        }
    }

    /// <summary>
    /// Znajduje najbli¿sze narzêdzie z listy nearbyTools i aktualizuje stan UI.
    /// </summary>
    private void UpdateClosestTool()
    {
        // Jeœli trzymamy narzêdzie, nie szukamy najbli¿szego
        if (heldTool != null)
        {
            // Jeœli 'closestTool' nie zosta³o wyczyszczone, zrób to
            if (closestTool != null)
            {
                closestTool = null;
                if (pickupTextUI != null) pickupTextUI.SetActive(false);
            }
            return; // WyjdŸ z funkcji
        }

        // Usuñ z listy narzêdzia, które mog³y zostaæ zniszczone (np. przez FixableScript)
        nearbyTools.RemoveAll(item => item == null);

        // U¿yj LINQ, aby posortowaæ listê po dystansie i wybraæ pierwsze (najbli¿sze)
        // FirstOrDefault() bezpiecznie zwróci 'null', jeœli lista 'nearbyTools' jest pusta.
        closestTool = nearbyTools
            .OrderBy(tool => Vector3.Distance(transform.position, tool.transform.position))
            .FirstOrDefault();

        // Zaktualizuj UI w zale¿noœci od tego, czy znaleŸliœmy jakieœ narzêdzie
        if (pickupTextUI != null)
        {
            pickupTextUI.SetActive(closestTool != null);
        }
    }

    /// <summary>
    /// Podpina narzêdzie do rêki gracza (WERSJA Z GripPoint LUB DOMYŒLNA).
    /// </summary>
    private void PickUpTool(GameObject toolToPickUp)
    {
        if (handSocket == null)
        {
            Debug.LogError("HandSocket nie jest przypisany! Nie mo¿na podnieœæ narzêdzia.");
            return;
        }

        heldTool = toolToPickUp;
        nearbyTools.Remove(heldTool); // Usuñ z listy "w pobli¿u"

        // --- Logika Grip: SprawdŸ, czy istnieje ToolGripSetup ---

        // Spróbuj pobraæ skrypt ToolGripSetup (lub jakkolwiek go nazwa³eœ)
        // **WA¯NE: Upewnij siê, ¿e nazwa 'ToolGripSetup' jest poprawna!**
        ToolGripPoint toolData = heldTool.GetComponent<ToolGripPoint>();

        // Ustaw rodzica (HandSocket)
        heldTool.transform.SetParent(handSocket);

        // Zastosuj transform w zale¿noœci od tego, czy skrypt istnieje
        if (toolData != null)
        {
            // Metoda 1: Narzêdzie MA skrypt ToolGripSetup
            // U¿yj zapisanych w nim wartoœci
            heldTool.transform.localPosition = toolData.gripLocalPosition;
            heldTool.transform.localRotation = toolData.gripLocalRotation;
        }
        else
        {
            // Metoda 2 (Fallback): Narzêdzie NIE MA skryptu
            // U¿yj domyœlnej pozycji (0,0,0) - tak jak prosi³eœ
            Debug.LogWarning("Narzêdzie " + heldTool.name + " nie ma skryptu 'ToolGripSetup'. U¿ywam domyœlnej pozycji (0,0,0).");
            heldTool.transform.localPosition = Vector3.zero;
            heldTool.transform.localRotation = Quaternion.identity;
        }
        // --- Koniec logiki Grip ---


        // Wy³¹cz fizykê grawitacji
        if (heldTool.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // W³¹cz ignorowanie kolizji miêdzy graczem a narzêdziem
        if (heldTool.TryGetComponent(out Collider col))
        {
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, col, true);
            }
        }
    }

    /// <summary>
    /// Upuszcza narzêdzie trzymane w rêce.
    /// </summary>
    private void DropTool()
    {
        if (heldTool == null) return; // Na wszelki wypadek

        GameObject toolToDrop = heldTool;
        heldTool = null; // Natychmiast wyczyœæ stan

        StartCoroutine(DropCooldown());

        // Odczep od rêki
        toolToDrop.transform.SetParent(null);

        // W³¹cz fizykê
        if (toolToDrop.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse); // Lekko odrzuæ
        }

        // Wy³¹cz ignorowanie kolizji
        if (toolToDrop.TryGetComponent(out Collider col))
        {
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, col, false);
            }
        }
    }

    // --- Wykrywanie Triggerów ---

    private void OnTriggerEnter(Collider other)
    {
        // Ignoruj, jeœli upuszczamy lub jeœli to ju¿ jest na liœcie
        if (other.CompareTag("Tool") && !isDropping && !nearbyTools.Contains(other.gameObject))
        {
            nearbyTools.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            nearbyTools.Remove(other.gameObject);
        }
    }

    // --- Coroutine ---

    /// <summary>
    /// Zapobiega natychmiastowemu ponownemu wykryciu upuszczonego narzêdzia.
    /// </summary>
    private IEnumerator DropCooldown()
    {
        isDropping = true;
        yield return new WaitForSeconds(0.1f);
        isDropping = false;
    }
}