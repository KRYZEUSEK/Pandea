using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject panel; // HerbariumPanel
    public GameObject button; // HerbariumPanel


    public void OpenPanel()
    {
        panel.SetActive(true);
        button.SetActive(false);

    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        button.SetActive(true);

    }
}
