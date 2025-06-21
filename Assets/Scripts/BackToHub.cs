using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BackToHub : MonoBehaviour
{
    [Header("Prompt Text")]
    [SerializeField] private TextMeshPro promptText3D;
    [SerializeField] private string message = "Voltar para o HUB [E]";
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Scene Settings")]
    [SerializeField] public string hubSceneName = "HubScene";

    [Header("Trigger Zone Settings")]
    [SerializeField] private bool isPromptZone = true;
    [SerializeField] private bool isInteractionZone = true;

    private bool playerInZone = false;
    private Transform playerCamera;

    private void Start()
    {
        if (promptText3D != null)
            promptText3D.gameObject.SetActive(false);

        if (Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (playerInZone)
        {
            if (isPromptZone && promptText3D != null)
            {
                promptText3D.gameObject.SetActive(true);
                promptText3D.text = message;

                // Face player camera
                Vector3 lookDir = promptText3D.transform.position - playerCamera.position;
                promptText3D.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            if (isInteractionZone && Input.GetKeyDown(interactionKey))
            {
                SceneManager.LoadScene(hubSceneName);
            }
        }
        else
        {
            if (promptText3D != null)
                promptText3D.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInZone = false;
    }

   public void LoadSceneByName(string sceneName)
{
    SceneManager.LoadScene(sceneName);
}
}
