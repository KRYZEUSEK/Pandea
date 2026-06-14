using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelLoader : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform contentParent;

    void Start()
    {
        LoadItems();
    }

    void LoadItems()
    {
        // JSON
        TextAsset jsonFile = Resources.Load<TextAsset>("Herbarium/items");

        if (jsonFile == null)
        {
            Debug.LogError("Nie znaleziono items.json w Resources/Herbarium");
            return;
        }

        ItemDataList data = JsonUtility.FromJson<ItemDataList>(jsonFile.text);

        foreach (ItemData item in data.items)
        {
            GameObject obj = Instantiate(itemPrefab, contentParent);

            obj.transform.Find("Name").GetComponent<TMP_Text>().text = item.name;
            obj.transform.Find("Description").GetComponent<TMP_Text>().text = item.description;

            // IMG
            Sprite sprite = Resources.Load<Sprite>("Herbarium/" + item.image);
            obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
        }
    }
}
