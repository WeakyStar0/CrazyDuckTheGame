using UnityEngine;
using DG.Tweening;

public class MenuPlayerAnim : MonoBehaviour
{
    [Tooltip("Offset from which the model will drop in.")]
    public float dropHeight = 2.0f;

    [Tooltip("Time the animation will take.")]
    public float duration = 1.0f;

    [Tooltip("Start animation on enable.")]
    public bool animateOnEnable = true;

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        if (animateOnEnable)
            PlayBounce();
    }

public void PlayBounce()
{
    transform.localPosition = originalPosition + new Vector3(0, dropHeight, 0);
    transform.DOLocalMoveY(originalPosition.y, duration)
             .SetEase(Ease.OutBounce)
             .SetUpdate(true); // ← This makes it ignore Time.timeScale
}

}
