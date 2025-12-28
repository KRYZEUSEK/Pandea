
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuUI : MonoBehaviour
{
    [Header("ród³a")]
    public BuildCatalog catalog;
    public BuildingManager buildingManager;
    public HotbarSelector hotbar;
    public InventoryObject inventory;

    [Header("UI")]
    public Transform contentParent;  // np. GridLayoutGroup / VerticalLayoutGroup
    public Button buttonPrefab;      // prosty Button z Image + Text/TMP

    [Header("Zachowanie")]
    public bool autoEnterBuildModeOnClick = true; // klik = od razu tryb budowy (jeœli wrench)

    void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        if (contentParent == null || buttonPrefab == null || catalog == null) return;

        // wyczyœæ stare
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // zbuduj nowe
        for (int i = 0; i < catalog.entries.Count; i++)
        {
            var data = catalog.entries[i];
            var btn = Instantiate(buttonPrefab, contentParent);

            // Ustaw ikonê i tekst
            
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

            // Klik: wybierz i ewentualnie odpal budowê
            btn.onClick.AddListener(() =>
            {
                buildingManager.SelectBuildable(data);

                if (autoEnterBuildModeOnClick)
                {
                    if (hotbar != null && hotbar.IsWrenchEquipped())
                        buildingManager.TryEnterBuildMode(data);
                    else
                        Debug.Log("Wybierz wrench w hotbarze, aby wejœæ w tryb budowy.");
                }
            });
        }
    }
}
