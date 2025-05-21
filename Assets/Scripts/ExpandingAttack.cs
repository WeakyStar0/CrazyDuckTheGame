using UnityEngine;
using System.Collections.Generic;

public class ExpandingAttack : MonoBehaviour
{
    public float expansionSpeed = 2f;
    public float maxScale = 10f;
    public float damageDuration = 3f;
    public int damageAmount = 1;

    private float timer;
    private HashSet<GameObject> damagedObjects = new HashSet<GameObject>();

    private Rigidbody rb;

    // Guarda a escala inicial dos filhos para manter proporção
    private Dictionary<Transform, Vector3> initialChildScales = new Dictionary<Transform, Vector3>();

    private void Start()
    {
        transform.localScale = new Vector3(0, 0, 1); // Z fixo
        timer = 0f;

        // Garante que os colliders filhos são trigger e guarda a escala original deles
        foreach (Transform child in transform)
        {
            Collider col = child.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            initialChildScales[child] = child.localScale;
        }

        // Rigidbody para triggers
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (transform.localScale.x < maxScale)
        {
            float scaleStep = expansionSpeed * Time.deltaTime;
            float newScaleXY = Mathf.Min(transform.localScale.x + scaleStep, maxScale);
            transform.localScale = new Vector3(newScaleXY, newScaleXY, 1); // Z fixo
                                                                           // **Não mexer na escala dos filhos**
        }

        timer += Time.deltaTime;
        if (timer >= damageDuration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameObject playerRoot = other.transform.root.gameObject;

        if (!damagedObjects.Contains(playerRoot))
        {
            PlayerHealth playerHealth = playerRoot.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform.position);
                damagedObjects.Add(playerRoot);
                Debug.Log("[Trigger] Dano aplicado ao player!");
            }
            else
            {
                Debug.LogWarning("[Trigger] PlayerHealth não encontrado no player!");
            }
        }
    }
}
