using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour 
{
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene"); // troca "GameScene" pelo nome da tua cena de jogo
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("CreditsScene"); // troca "CreditsScene" pelo nome da tua cena de créditos
    }
}
//código do menu, vai se adicionar mais cenas em caso de termos mis botões - mark