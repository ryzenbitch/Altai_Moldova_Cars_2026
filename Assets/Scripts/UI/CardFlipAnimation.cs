using UnityEngine;
using System.Collections;

/// <summary>
/// Handles card flip animation using scale trick (no Animator required).
/// Attach to a card UI GameObject.
/// </summary>
public class CardFlipAnimation : MonoBehaviour
{
    [Header("Settings")]
    public float flipDuration = 0.35f;
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("References")]
    public GameObject frontSide;
    public GameObject backSide;

    private RectTransform rectTransform;
    private bool isFlipping = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void FlipToFront(System.Action onComplete = null)
    {
        if (isFlipping) return;
        StartCoroutine(DoFlip(true, onComplete));
    }

    public void FlipToBack(System.Action onComplete = null)
    {
        if (isFlipping) return;
        StartCoroutine(DoFlip(false, onComplete));
    }

    IEnumerator DoFlip(bool showFront, System.Action onComplete)
    {
        isFlipping = true;
        float half = flipDuration / 2f;

        // First half: scale X to 0
        float t = 0;
        Vector3 startScale = rectTransform.localScale;
        while (t < half)
        {
            t += Time.deltaTime;
            float progress = flipCurve.Evaluate(t / half);
            rectTransform.localScale = new Vector3(
                Mathf.Lerp(startScale.x, 0f, progress),
                startScale.y, startScale.z);
            yield return null;
        }

        // Swap sides at midpoint
        if (frontSide) frontSide.SetActive(showFront);
        if (backSide) backSide.SetActive(!showFront);

        // Second half: scale X back to 1
        t = 0;
        while (t < half)
        {
            t += Time.deltaTime;
            float progress = flipCurve.Evaluate(t / half);
            rectTransform.localScale = new Vector3(
                Mathf.Lerp(0f, 1f, progress),
                startScale.y, startScale.z);
            yield return null;
        }

        rectTransform.localScale = Vector3.one;
        isFlipping = false;
        onComplete?.Invoke();
    }

    public void ShowFrontInstant()
    {
        if (frontSide) frontSide.SetActive(true);
        if (backSide) backSide.SetActive(false);
        rectTransform.localScale = Vector3.one;
    }

    public void ShowBackInstant()
    {
        if (frontSide) frontSide.SetActive(false);
        if (backSide) backSide.SetActive(true);
        rectTransform.localScale = Vector3.one;
    }
}
