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
        SceneManager.LoadScene(SceneName); 
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene(SceneCredits); 
    }
}
