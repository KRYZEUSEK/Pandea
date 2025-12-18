using UnityEngine;

public class SceneChanger : MonoBehaviour {
    // wszystkie sceny powinny byc w ustawieniach builda
    [SerializeField] private Object[] scenes;

    public void ChangeScene(int sceneIndex) {
        if (sceneIndex < 0 || sceneIndex >= scenes.Length) {
            Debug.LogError("Scene index out of range!");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIndex].name);
    }
}