using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuUI : MonoBehaviour
{
    [Header("èrÛd≥a")]
    public BuildCatalog catalog;
    public BuildingManager buildingManager; // Moøesz to pole zostawiÊ, ale skrypt sam je znajdzie
    public HotbarSelector hotbar;           // To teø samo siÍ znajdzie
    public InventoryObject inventory;       // I to teø

    [Header("UI")]
    public Transform contentParent;  // np. GridLayoutGroup / VerticalLayoutGroup
    public Button buttonPrefab;      // prosty Button z Image + Text/TMP

    [Header("Zachowanie")]
    public bool autoEnterBuildModeOnClick = true; // klik = od razu tryb budowy (jeúli wrench)

    private void Awake()
    {
        // PrÛbujemy znaleüÊ niezbÍdne skrypty juø na starcie, jeúli nie zosta≥y przypisane
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryObject>(FindObjectsInactive.Include);

        // Zauwaø: buildingManager i hotbar mogπ nie byÊ tu jeszcze dostÍpne (jeúli gracz siÍ dopiero ≥aduje),
        // dlatego w Rebuild i przy klikaniu bÍdziemy tego dodatkowo pilnowaÊ.
    }

    void OnEnable()
    {
        // Na wszelki wypadek ponawiamy szukanie, gdy UI jest w≥πczane (gracz mÛg≥ zostaÊ w≥aúnie zespawnowany)
        FindPlayerReferences();

        // Podpinamy siÍ pod event zmiany ekwipunku
        if (inventory != null)
        {
            // Odepnij najpierw, na wypadek gdyby coú podpiÍ≥o dwa razy
            inventory.OnInventoryChanged -= Rebuild;
            inventory.OnInventoryChanged += Rebuild;
        }

        Rebuild(); // Pierwsze zbudowanie menu po w≥πczeniu
    }

    void OnDisable()
    {
        // Odpinamy siÍ, gdy wy≥πczamy menu (bardzo waøne dla optymalizacji!)
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Rebuild;
        }
    }

    // --- Nowa metoda pomocnicza ---
    private void FindPlayerReferences()
    {
        if (buildingManager == null)
            buildingManager = FindFirstObjectByType<BuildingManager>(FindObjectsInactive.Include);

        if (hotbar == null)
            hotbar = FindFirstObjectByType<HotbarSelector>(FindObjectsInactive.Include);
    }

    public void Rebuild()
    {
        // Szukamy ponownie na wypadek odúwieøania menu.
        FindPlayerReferences();

        if (contentParent == null || buttonPrefab == null || catalog == null) return;

        // Jeúli ekwipunek nie zdπøy≥ siÍ przypisaÊ, przerwij (zapobiegnie to b≥Ídom wyúwietlania kosztÛw).
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<InventoryObject>(FindObjectsInactive.Include);
            if (inventory == null) return;
        }

        // wyczyúÊ stare
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // zbuduj nowe
        for (int i = 0; i < catalog.entries.Count; i++)
        {
            var data = catalog.entries[i];
            var btn = Instantiate(buttonPrefab, contentParent);

            var tmp = btn.GetComponentInChildren<TMP_Text>(true);

            if (tmp != null)
            {
                string buttonText = data.id + "\n";

                foreach (var x in data.costs)
                {
                    var slot = inventory.Slots
                        .FirstOrDefault(y => y.item != null && y.item.id == x.item.id);

                    int ownedAmount = slot != null ? slot.amount : 0;

                    buttonText += $"<size=80%>{ownedAmount}/{x.amount} {x.item.id}\n";
                }

                tmp.text = buttonText;
            }
            else
            {
                Debug.LogWarning("TMP_Text not found on button");
            }

            // Klik: wybierz i ewentualnie odpal budowÍ
            btn.onClick.AddListener(() =>
            {
                // Przed wykonaniem akcji ZAWSZE upewniamy siÍ, øe mamy referencje. 
                // Gracz mÛg≥ "zginπÊ" lub odrodziÊ siÍ w trakcie wyúwietlania panelu.
                FindPlayerReferences();

                if (buildingManager != null)
                {
                    buildingManager.SelectBuildable(data);

                    if (autoEnterBuildModeOnClick)
                    {
                        if (hotbar != null && hotbar.IsWrenchEquipped())
                            buildingManager.TryEnterBuildMode(data);
                        else
                            Debug.Log("Wybierz wrench w hotbarze, aby wejúÊ w tryb budowy.");
                    }
                }
                else
                {
                    Debug.LogError("Nie znaleziono BuildingManagera! KlikniÍcie anulowane.");
                }
            });
        }
    }
}