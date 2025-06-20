using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Transform))]
public class WiggleBounce : MonoBehaviour
{
    [Header("Wiggle Settings")]
    public bool enableWiggle = true;
    [Range(-5f, 5f)] public float wiggleAmount = 1f;
    [Range(-5f, 5f)] public float wiggleSpeed = 1f;
    [Range(-5f, 1f)] public float wiggleRandomness = 0.5f;

    [Header("Bounce Settings")]
    public bool enableBounce = true;
    [Range(-5f, 5f)] public float bounceHeight = 0.5f;
    [Range(0.1f, 5f)] public float bounceSpeed = 1f;
    [Range(0f, 1f)] public float bounceSquash = 0.2f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Sequence wiggleSequence;
    private Sequence bounceSequence;
    private bool isInitialized = false;

    void Start()
    {
        InitializeAnimations();
    }

    void InitializeAnimations()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        isInitialized = true;

        CreateWiggleSequence();
        CreateBounceSequence();

        if (enableWiggle) wiggleSequence.Play();
        if (enableBounce) bounceSequence.Play();
    }

    void CreateWiggleSequence()
    {
        if (wiggleSequence != null && wiggleSequence.IsActive())
        {
            wiggleSequence.Kill();
        }

        wiggleSequence = DOTween.Sequence();
        wiggleSequence.Append(transform.DOLocalMoveX(wiggleAmount * 0.5f, 0.5f / wiggleSpeed).SetRelative());
        wiggleSequence.Append(transform.DOLocalMoveX(-wiggleAmount, 0.75f / wiggleSpeed).SetRelative());
        wiggleSequence.Append(transform.DOLocalMoveX(wiggleAmount, 0.75f / wiggleSpeed).SetRelative());
        wiggleSequence.Append(transform.DOLocalMoveX(-wiggleAmount * 0.5f, 0.5f / wiggleSpeed).SetRelative());
        wiggleSequence.SetLoops(-1, LoopType.Yoyo);
        wiggleSequence.SetEase(Ease.InOutSine);
        wiggleSequence.OnUpdate(() => {
            if (wiggleRandomness > 0)
            {
                float randomOffset = Mathf.Lerp(-wiggleRandomness, wiggleRandomness, Random.value);
                transform.localPosition += new Vector3(randomOffset * wiggleAmount * 0.05f, 0, 0);
            }
        });
        wiggleSequence.Pause();
    }

    void CreateBounceSequence()
    {
        if (bounceSequence != null && bounceSequence.IsActive())
        {
            bounceSequence.Kill();
        }

        bounceSequence = DOTween.Sequence();
        bounceSequence.Append(transform.DOLocalMoveY(bounceHeight, 0.4f / bounceSpeed).SetRelative());
        bounceSequence.Join(transform.DOScaleY(originalScale.y - bounceSquash, 0.2f / bounceSpeed));
        bounceSequence.Append(transform.DOLocalMoveY(-bounceHeight, 0.4f / bounceSpeed).SetRelative());
        bounceSequence.Join(transform.DOScaleY(originalScale.y + (bounceSquash * 0.3f), 0.2f / bounceSpeed));
        bounceSequence.Append(transform.DOScaleY(originalScale.y, 0.2f / bounceSpeed));
        bounceSequence.SetLoops(-1, LoopType.Restart);
        bounceSequence.SetEase(Ease.OutQuad);
        bounceSequence.Pause();
    }

    void OnValidate()
    {
        if (!isInitialized || !Application.isPlaying) return;

        if (enableWiggle || wiggleSequence == null || !wiggleSequence.IsActive())
        {
            CreateWiggleSequence();
            if (enableWiggle) wiggleSequence.Play();
        }

        if (enableBounce || bounceSequence == null || !bounceSequence.IsActive())
        {
            CreateBounceSequence();
            if (enableBounce) bounceSequence.Play();
        }
    }

    void OnDestroy()
    {
        wiggleSequence?.Kill();
        bounceSequence?.Kill();
    }

    void OnDisable()
    {
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            if (enableWiggle) wiggleSequence?.Restart();
            if (enableBounce) bounceSequence?.Restart();
        }
    }
}