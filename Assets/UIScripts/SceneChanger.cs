using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{

    public void ChangeScene(int buildIndex)
    {
        // £aduje scenê na podstawie jej numeru (indeksu) w File -> Build Settings
        SceneManager.LoadScene(buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}