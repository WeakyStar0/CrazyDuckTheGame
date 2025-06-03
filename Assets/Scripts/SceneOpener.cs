using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneOpener : MonoBehaviour
{
    [Tooltip("Name of the scene to load. Make sure it is added to Build Settings.")]
    [SerializeField] private string sceneName = "TestScene";

    public void OpenScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is not set!");
        }
    }
}
