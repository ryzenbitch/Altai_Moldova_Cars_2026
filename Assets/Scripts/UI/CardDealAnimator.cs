using UnityEngine;
using System.Collections;

/// <summary>
/// Animates a card flying from a source position to a target position.
/// Attach to a card prefab or use via CardDealAnimator.
/// </summary>
public class CardDealAnimator : MonoBehaviour
{
    public static CardDealAnimator Instance { get; private set; }

    [Header("Deal Settings")]
    public float dealDuration = 0.4f;
    public AnimationCurve dealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float arcHeight = 80f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Moves a UI element from startPos to endPos in an arc.
    /// </summary>
    public void AnimateCard(RectTransform card, Vector2 startPos, Vector2 endPos,
        System.Action onComplete = null)
    {
        StartCoroutine(MoveCard(card, startPos, endPos, onComplete));
    }

    IEnumerator MoveCard(RectTransform card, Vector2 start, Vector2 end, System.Action onComplete)
    {
        float t = 0;
        while (t < dealDuration)
        {
            t += Time.deltaTime;
            float progress = dealCurve.Evaluate(t / dealDuration);

            // Arc using sine
            float arc = Mathf.Sin(progress * Mathf.PI) * arcHeight;
            Vector2 pos = Vector2.Lerp(start, end, progress);
            pos.y += arc;

            card.anchoredPosition = pos;
            yield return null;
        }
        card.anchoredPosition = end;
        onComplete?.Invoke();
    }
}
