using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Shows all cards side-by-side after a round, crowns the winner,
/// then auto-hides after a delay.
/// </summary>
public class RoundResultPanel : MonoBehaviour
{
    [Header("Per-player display (up to 3)")]
    public CardDisplayUI[] resultCardDisplays; // 3 slots
    public TextMeshProUGUI[] resultPlayerNames;  // 3 labels
    public GameObject[] winnerCrowns;        // 3 crown icons

    [Header("Banner")]
    public TextMeshProUGUI roundResultHeadline;
    public TextMeshProUGUI selectedStatText;

    [Header("Continue")]
    public Button continueButton;
    [Tooltip("Seconds before auto-hide. Set 0 to disable.")]
    public float autoHideDelay = 4f;

    // NOTE: auto-hide coroutine is intentionally NOT started here.
    // The object may still be inside an inactive parent at call time,
    // which would cause "Coroutine couldn't be started because the game
    // object is inactive". The caller (GameSceneController) starts the
    // delayed Hide() from its own always-active context instead.

    void Awake()
    {
        gameObject.SetActive(false);
        if (continueButton) continueButton.onClick.AddListener(Hide);
    }

    public void ShowResult(PlayerData[] players, CardData[] snapshot,
                           StatType stat, int winnerIdx)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();

        // Stat banner
        if (selectedStatText)
            selectedStatText.text = $"Характеристика: {stat.GetRussianName()}";

        // Headline
        bool localWon = (winnerIdx == 0);
        if (roundResultHeadline)
        {
            roundResultHeadline.text = localWon ? "🏆 ВЫ ПОБЕДИЛИ В РАУНДЕ!" : $"Победил: {players[winnerIdx].nickname}";
            roundResultHeadline.color = localWon
                ? new Color(1f, 0.84f, 0f)
                : new Color(0.9f, 0.3f, 0.3f);
        }

        // Cards
        for (int i = 0; i < resultCardDisplays.Length; i++)
        {
            bool hasCard = i < players.Length && snapshot[i] != null;
            resultCardDisplays[i].gameObject.SetActive(hasCard);

            if (!hasCard)
            {
                if (i < winnerCrowns.Length && winnerCrowns[i]) winnerCrowns[i].SetActive(false);
                continue;
            }

            resultCardDisplays[i].ShowCard(snapshot[i], false);
            resultCardDisplays[i].HighlightStat(stat, i == winnerIdx);

            if (i < resultPlayerNames.Length && resultPlayerNames[i])
                resultPlayerNames[i].text = players[i].nickname;

            if (i < winnerCrowns.Length && winnerCrowns[i])
                winnerCrowns[i].SetActive(i == winnerIdx);
        }

        // Auto-hide is triggered by the caller — see GameSceneController.HandleRoundResolved
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
