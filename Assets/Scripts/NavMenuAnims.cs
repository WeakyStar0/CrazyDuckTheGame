using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NavMenuAnims : MonoBehaviour
{
    [System.Serializable]
    public class Interaction
    {
        public string name = "Interaction";
        public Vector3 targetScale = Vector3.one;
        public Transform targetPosition;
        public float duration = 0.5f;
        public Ease easeType = Ease.OutBack;
        [Tooltip("Should it return to original state after completing?")]
        public bool returnToOriginal = false;
        [Tooltip("Delay before returning to original state")]
        public float returnDelay = 0f;
    }

    [Header("Object to Animate")]
    public Transform objectToAnimate;

    [Header("Interactions")]
    public Interaction[] interactions;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Sequence currentAnimation;
    private string currentInteractionName = null;

    void Awake()
    {
        if (objectToAnimate == null)
            objectToAnimate = transform;

        originalPosition = objectToAnimate.position;
        originalScale = objectToAnimate.localScale;
    }

    public void PlayInteractionByName(string interactionName)
    {
        // Toggle logic
        if (currentInteractionName == interactionName)
        {
            SmoothResetToOriginal();
            currentInteractionName = null;
            return;
        }

        foreach (var interaction in interactions)
        {
            if (interaction.name == interactionName)
            {
                ExecuteInteraction(interaction);
                currentInteractionName = interaction.name;
                return;
            }
        }

        Debug.LogWarning($"Interaction '{interactionName}' not found in {gameObject.name}.");
    }

    void ExecuteInteraction(Interaction interaction)
    {
        if (currentAnimation != null && currentAnimation.IsActive())
            currentAnimation.Kill();

        Vector3 targetPos = interaction.targetPosition != null ?
            interaction.targetPosition.position : originalPosition;

        currentAnimation = DOTween.Sequence().SetUpdate(true);

        currentAnimation.Append(objectToAnimate
            .DOScale(interaction.targetScale, interaction.duration)
            .SetEase(interaction.easeType)
            .SetUpdate(true))
        .Join(objectToAnimate
            .DOMove(targetPos, interaction.duration)
            .SetEase(interaction.easeType)
            .SetUpdate(true));

        if (interaction.returnToOriginal)
        {
            currentAnimation.AppendInterval(interaction.returnDelay)
                .Append(objectToAnimate
                    .DOScale(originalScale, interaction.duration)
                    .SetEase(interaction.easeType)
                    .SetUpdate(true))
                .Join(objectToAnimate
                    .DOMove(originalPosition, interaction.duration)
                    .SetEase(interaction.easeType)
                    .SetUpdate(true))
                .OnComplete(() => currentInteractionName = null);
        }
    }

    void SmoothResetToOriginal()
    {
        if (currentAnimation != null && currentAnimation.IsActive())
            currentAnimation.Kill();

        currentAnimation = DOTween.Sequence().SetUpdate(true);

        currentAnimation.Append(objectToAnimate
            .DOScale(originalScale, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true))
        .Join(objectToAnimate
            .DOMove(originalPosition, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true))
        .OnComplete(() => currentInteractionName = null);
    }

    void OnDestroy()
    {
        if (currentAnimation != null && currentAnimation.IsActive())
            currentAnimation.Kill();
    }
}
