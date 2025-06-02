using System.Collections;
using UnityEngine;

public class GlitchControl : MonoBehaviour
{
    [Header("Glitch Settings")]
    [Tooltip("How often should the glitch effect happen (higher = more frequent)")]
    public float glitchChance = 0.1f;

    [Header("Linked Light (Optional)")]
    [Tooltip("Optional light to flash with the glitch effect")]
    public Light linkedLight;

    [Tooltip("Minimum and maximum intensity multiplier during glitch")]
    public Vector2 lightGlitchIntensityRange = new Vector2(0.2f, 0.6f);

    private Material hologramMaterial;
    private float originalGlowIntensity;
    private float originalLightIntensity;

    private WaitForSeconds glitchLoopWait = new WaitForSeconds(0.1f);

    void Awake()
    {
        hologramMaterial = GetComponent<Renderer>().material;

        if (linkedLight != null)
            originalLightIntensity = linkedLight.intensity;
    }

    IEnumerator Start()
    {
        originalGlowIntensity = hologramMaterial.GetFloat("_GlowIntensity");

        while (true)
        {
            float glitchTest = Random.Range(0f, 1f);

            if (glitchTest <= glitchChance)
            {
                // Apply material glitch
                float glitchIntensity = Random.Range(0.07f, 0.1f);
                float glowIntensity = originalGlowIntensity * Random.Range(0.14f, 0.44f);
                hologramMaterial.SetFloat("_GlitchIntensity", glitchIntensity);
                hologramMaterial.SetFloat("_GlowIntensity", glowIntensity);

                // Flash the light
                if (linkedLight != null)
                {
                    linkedLight.intensity = originalLightIntensity * Random.Range(lightGlitchIntensityRange.x, lightGlitchIntensityRange.y);
                }

                // Glitch duration
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

                // Reset both
                hologramMaterial.SetFloat("_GlitchIntensity", 0f);
                hologramMaterial.SetFloat("_GlowIntensity", originalGlowIntensity);

                if (linkedLight != null)
                    linkedLight.intensity = originalLightIntensity;
            }

            yield return glitchLoopWait;
        }
    }
}
