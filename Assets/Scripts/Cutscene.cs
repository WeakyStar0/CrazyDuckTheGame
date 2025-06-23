using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    public Image cutsceneImage;
    public TMP_Text cutsceneText;

    public Sprite[] images;

    [TextArea(2, 5)] public string[] texts;

    // NOVO: Durações personalizadas para cada frame
    public float[] durations;

    public float typingSpeed = 0.05f;
    public float fadeDuration = 1f;

    public string nextSceneName = "";
    public AudioSource typingAudioSource;

    private int currentIndex = 0;

    void Start()
    {
        SetAlpha(0f);
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        while (currentIndex < images.Length)
        {
            cutsceneImage.sprite = images[currentIndex];

            yield return StartCoroutine(FadeIn());
            yield return StartCoroutine(TypeText(texts[currentIndex]));

            // NOVO: Aguarda o tempo definido para este índice
            float waitTime = 0f;
            if (durations != null && currentIndex < durations.Length)
            {
                waitTime = durations[currentIndex];
            }
            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(FadeOut());
            currentIndex++;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator TypeText(string text)
    {
        cutsceneText.text = "";
        if (typingAudioSource != null) typingAudioSource.Play();

        foreach (char c in text)
        {
            cutsceneText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (typingAudioSource != null) typingAudioSource.Stop();
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1f);
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float alpha)
    {
        Color imgColor = cutsceneImage.color;
        cutsceneImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, alpha);

        Color txtColor = cutsceneText.color;
        cutsceneText.color = new Color(txtColor.r, txtColor.g, txtColor.b, alpha);
    }
}
