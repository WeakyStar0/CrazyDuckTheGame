using UnityEngine;
using TMPro;

public class ModelSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ModelData
    {
        public GameObject modelObject;      // Model already in the scene
        public Vector3 positionOffset;      // Position offset for fine adjustment
        public Quaternion rotationOffset;   // Rotation offset for orientation
        public string modelName;            // Name to display
    }

    public ModelData[] models;
    public Transform basePosition;          // Position/rotation base point
    public TextMeshPro nameDisplayTMP;      // 3D TextMeshPro object

    private int currentIndex = -1;
    private bool playerInRange = false;

    void Start()
    {
        HideAllModels();
        currentIndex = -1;
        SwitchToNextModel(); // Show first model on start
    }


    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
            SwitchToNextModel();

        if (Input.GetKey(KeyCode.E))
            RotateCurrentModel(-1);

        if (Input.GetKey(KeyCode.Q))
            RotateCurrentModel(1);
    }

    void HideAllModels()
    {
        foreach (var data in models)
        {
            if (data.modelObject != null)
                data.modelObject.SetActive(false);
        }
    }

    void SwitchToNextModel()
    {
        // Hide current
        if (currentIndex >= 0 && currentIndex < models.Length)
            models[currentIndex].modelObject.SetActive(false);

        // Move to next
        currentIndex = (currentIndex + 1) % models.Length;

        var data = models[currentIndex];
        GameObject model = data.modelObject;

        if (model != null)
        {
            model.transform.position = basePosition.position + data.positionOffset;
            model.transform.rotation = basePosition.rotation * data.rotationOffset;
            model.SetActive(true);
        }

        // Update text
        if (nameDisplayTMP != null)
            nameDisplayTMP.text = data.modelName;
    }

    void RotateCurrentModel(int direction)
    {
        if (currentIndex < 0 || currentIndex >= models.Length)
            return;

        var model = models[currentIndex].modelObject;
        if (model != null)
            model.transform.Rotate(Vector3.up, direction * 100f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
