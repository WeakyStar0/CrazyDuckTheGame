using UnityEngine;

public class FloatingChildren : MonoBehaviour
{
    public float floatAmplitude = 0.2f; // How far up and down
    public float floatFrequency = 1f;   // How fast it moves

    private Vector3[] initialPositions;

    void Start()
    {
        int childCount = transform.childCount;
        initialPositions = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            initialPositions[i] = transform.GetChild(i).localPosition;
        }
    }

    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 startPos = initialPositions[i];

            float offset = Mathf.Sin(Time.time * floatFrequency + i) * floatAmplitude;
            child.localPosition = startPos + new Vector3(0f, offset, 0f);
        }
    }
}
