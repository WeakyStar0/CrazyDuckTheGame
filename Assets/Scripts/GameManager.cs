using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Text collectibleCounterText;
    private int collectibleCount = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void CollectItem()
    {
        collectibleCount++;
        UpdateCounterUI();
    }
    
    void UpdateCounterUI()
    {
        if (collectibleCounterText != null)
        {
            collectibleCounterText.text = "Coletáveis: " + collectibleCount;
        }
    }
}