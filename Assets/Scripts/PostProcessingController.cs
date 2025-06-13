using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // or PostProcessing if using PP Stack v2

public class PostProcessingController : MonoBehaviour
{
    public Volume postProcessingVolume;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    private void Awake()
    {
        if (postProcessingVolume == null)
        {
            postProcessingVolume = GetComponent<Volume>();
            if (postProcessingVolume == null)
            {
                Debug.LogError("PostProcessingController: No Volume component assigned or found.");
                enabled = false;
                return;
            }
        }

        // Try to get vignette and chromatic aberration overrides
        if (!postProcessingVolume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.LogWarning("Vignette not found in post-processing profile.");
        }

        if (!postProcessingVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            Debug.LogWarning("Chromatic Aberration not found in post-processing profile.");
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
        {
            vignette.active = intensity > 0f;
            vignette.intensity.value = Mathf.Clamp01(intensity);
        }
    }

    public void SetChromaticAberrationIntensity(float intensity)
    {
        if (chromaticAberration != null)
        {
            chromaticAberration.active = intensity > 0f;
            chromaticAberration.intensity.value = Mathf.Clamp01(intensity);
        }
    }
}
