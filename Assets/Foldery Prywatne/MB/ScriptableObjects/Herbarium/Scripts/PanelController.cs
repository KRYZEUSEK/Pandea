using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject panel; // HerbariumPanel
    public GameObject button; // HerbariumPanel
    public HotbarSelector hotbarScript;

    public void OpenPanel()
    {
        panel.SetActive(true);
        button.SetActive(false);
        if (hotbarScript != null) hotbarScript.enabled = false;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        button.SetActive(true);
        if (hotbarScript != null) hotbarScript.enabled = true;
    }
}
