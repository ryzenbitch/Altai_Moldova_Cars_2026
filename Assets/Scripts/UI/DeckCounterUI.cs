using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Shows deck and reserve count. Numbers "pop" on change.
/// </summary>
public class DeckCounterUI : MonoBehaviour
{
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI reserveCountText;

    int lastDeck = -1;
    int lastReserve = -1;

    public void UpdateCounts(int deck, int reserve)
    {
        if (deck != lastDeck)
        {
            lastDeck = deck;
            if (deckCountText) deckCountText.text = $"Колода: {deck}";
            if (deckCountText && gameObject.activeInHierarchy)
                StartCoroutine(Pop(deckCountText.transform));
        }
        if (reserve != lastReserve)
        {
            lastReserve = reserve;
            if (reserveCountText) reserveCountText.text = $"Резерв: {reserve}";
            if (reserveCountText && gameObject.activeInHierarchy)
                StartCoroutine(Pop(reserveCountText.transform));
        }
    }

    IEnumerator Pop(Transform t)
    {
        float dur = 0.18f, half = dur / 2f, elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.one * Mathf.Lerp(1f, 1.3f, elapsed / half);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, elapsed / half);
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}