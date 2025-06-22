using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public class MenuManager : MonoBehaviour 
{
    [Header ("scene")]
    [SerializeField] public string SceneName = "Tutorial";
    [SerializeField] public string SceneCredits = "Credits";

    public void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneName); 
    }

    public void LoadCredits()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneCredits); 
    }
}
