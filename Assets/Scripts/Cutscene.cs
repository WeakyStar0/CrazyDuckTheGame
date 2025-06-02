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

    [TextArea(2, 5)] public string[] texts;  // Allows multi-line text in Inspector

    public AudioClip[] audioClips;  // <-- Add this new array for audio clips

    public float typingSpeed = 0.05f;
    public float fadeDuration = 1f;

    [Tooltip("Tempo em segundos para avançar automaticamente. 0 = só avança com espaço.")]
    public float autoAdvanceTime = 0f;

    [Tooltip("Nome ou índice da próxima cena a ser carregada ao final da cutscene.")]
    public string nextSceneName = "";

    public AudioSource typingAudioSource; // Typing sound

    public AudioSource audioSource;  // <-- Add an AudioSource to play clips for each step

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

            // Play the audio clip for the current step if assigned
            if (audioSource != null && audioClips != null && currentIndex < audioClips.Length && audioClips[currentIndex] != null)
            {
                audioSource.Stop();
                audioSource.clip = audioClips[currentIndex];
                audioSource.Play();
            }

            yield return StartCoroutine(FadeIn());

            yield return StartCoroutine(TypeText(texts[currentIndex]));

            yield return new WaitForSeconds(0.1f);

            float timer = 0f;
            bool advanced = false;
            while (!advanced)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    advanced = true;
                }

                if (autoAdvanceTime > 0f && timer >= autoAdvanceTime)
                {
                    advanced = true;
                }

                timer += Time.deltaTime;
                yield return null;
            }

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

        if (typingAudioSource != null)
            typingAudioSource.Play();

        foreach (char c in text)
        {
            cutsceneText.text += c;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                cutsceneText.text = text;
                break;
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        if (typingAudioSource != null)
            typingAudioSource.Stop();
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
