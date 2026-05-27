using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour
{
    [Header("Table Layout")]
    public RectTransform tableRoot;
    public PlayerSlotUI[] playerSlots; // 3 slots

    [Header("Card Areas")]
    public CardDisplayUI myCardDisplay;
    public CardDisplayUI[] opponentCardDisplays; // up to 2

    [Header("Stat Selection Panel")]
    public GameObject statSelectionPanel;
    public Button[] statButtons; // 5 buttons
    public TextMeshProUGUI[] statButtonLabels;
    public TextMeshProUGUI[] statButtonValues;

    [Header("Round Result Panel")]
    public GameObject roundResultPanel;
    public TextMeshProUGUI roundResultText;
    public Image winnerAvatar;
    public TextMeshProUGUI winnerNameText;

    [Header("HUD")]
    public TextMeshProUGUI[] playerCardCounts; // deck + reserve per player
    public TextMeshProUGUI roundNumberText;
    public TextMeshProUGUI turnIndicatorText;
    public GameObject myTurnIndicator;

    [Header("Elimination / Game Over")]
    public GameObject eliminationPanel;
    public TextMeshProUGUI eliminationText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverWinnerText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip cardDealSfx;
    public AudioClip cardFlipSfx;
    public AudioClip winSfx;
    public AudioClip loseSfx;
    public AudioClip buttonSfx;

    private GameManager gm;
    private int localPlayerIndex = 0;

    void Start()
    {
        gm = GameManager.Instance;

        gm.OnStateChanged += HandleStateChange;
        gm.OnTurnChanged += HandleTurnChange;
        gm.OnRoundResolved += HandleRoundResolved;
        gm.OnPlayerEliminated += HandlePlayerEliminated;
        gm.OnGameOver += HandleGameOver;
        gm.OnCardsDealt += HandleCardsDealt;

        SetupPlayerSlots();
        statSelectionPanel.SetActive(false);
        roundResultPanel.SetActive(false);
        eliminationPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        gm.StartGame();

        playAgainButton.onClick.AddListener(PlayAgain);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void OnDestroy()
    {
        if (gm == null) return;
        gm.OnStateChanged -= HandleStateChange;
        gm.OnTurnChanged -= HandleTurnChange;
        gm.OnRoundResolved -= HandleRoundResolved;
        gm.OnPlayerEliminated -= HandlePlayerEliminated;
        gm.OnGameOver -= HandleGameOver;
        gm.OnCardsDealt -= HandleCardsDealt;
    }

    void SetupPlayerSlots()
    {
        var players = GameSettings.Players;
        localPlayerIndex = 0; // Local player always index 0

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < players.Length)
            {
                playerSlots[i].gameObject.SetActive(true);
                playerSlots[i].Setup(players[i]);
            }
            else
            {
                playerSlots[i].gameObject.SetActive(false);
            }
        }
    }

    void HandleCardsDealt()
    {
        UpdateAllCardCounts();
        if (roundNumberText) roundNumberText.text = "Раунд 1";
    }

    void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.PlayerTurn:
                statSelectionPanel.SetActive(false);
                roundResultPanel.SetActive(false);
                UpdateAllCardCounts();
                bool isMyTurn = gm.currentPlayerIndex == localPlayerIndex;
                if (myTurnIndicator) myTurnIndicator.SetActive(isMyTurn);
                if (isMyTurn)
                {
                    // Show "your turn" and auto-draw card
                    StartCoroutine(DrawCardForCurrentPlayer());
                }
                break;

            case GameState.SelectingStat:
                if (gm.currentPlayerIndex == localPlayerIndex)
                {
                    ShowStatSelection(gm.currentRoundCards[localPlayerIndex]);
                }
                break;

            case GameState.RevealingCards:
                statSelectionPanel.SetActive(false);
                RevealOpponentCards();
                break;
        }
    }

    IEnumerator DrawCardForCurrentPlayer()
    {
        yield return new WaitForSeconds(0.3f);
        PlaySFX(cardDealSfx);
        // Draw and show the card
        gm.PlayerReadyToSelectStat();
        myCardDisplay.ShowCard(gm.currentRoundCards[localPlayerIndex]);
    }

    void ShowStatSelection(CardData card)
    {
        statSelectionPanel.SetActive(true);
        StatType[] stats = { StatType.Acceleration, StatType.Horsepower, StatType.MaxSpeed, StatType.EngineVolume, StatType.Weight };

        for (int i = 0; i < statButtons.Length; i++)
        {
            StatType stat = stats[i];
            if (statButtonLabels[i]) statButtonLabels[i].text = stat.GetRussianName();
            if (statButtonValues[i])
            {
                float val = stat.GetValue(card);
                string unit = stat == StatType.Acceleration ? " сек" : stat == StatType.MaxSpeed ? " км/ч" : stat == StatType.EngineVolume ? " см³" : stat == StatType.Weight ? " кг" : " л.с.";
                string indicator = stat.IsLessIsBetter() ? "↓ меньше лучше" : "↑ больше лучше";
                statButtonValues[i].text = $"{val}{unit}\n<size=60%>{indicator}</size>";
            }

            int capturedIdx = i;
            statButtons[i].onClick.RemoveAllListeners();
            statButtons[i].onClick.AddListener(() => {
                PlaySFX(buttonSfx);
                gm.SelectStat(stats[capturedIdx]);
            });
        }
    }

    void RevealOpponentCards()
    {
        for (int i = 0; i < gm.players.Length; i++)
        {
            if (i == localPlayerIndex) continue;
            int opSlot = i > localPlayerIndex ? i - 1 : i;
            if (opSlot < opponentCardDisplays.Length)
            {
                PlaySFX(cardFlipSfx);
                opponentCardDisplays[opSlot].ShowCard(gm.currentRoundCards[i]);
            }
        }
    }

    void HandleTurnChange(int playerIndex)
    {
        UpdateAllCardCounts();
        string name = gm.players[playerIndex].nickname;
        if (turnIndicatorText) turnIndicatorText.text = $"Ходит: {name}";
        if (roundNumberText) roundNumberText.text = $"Раунд {gm.roundNumber + 1}";

        // Hide opponent cards from previous round
        foreach (var disp in opponentCardDisplays) disp.HideCard();
        myCardDisplay.HideCard();
    }

    void HandleRoundResolved(CardData[] cards, StatType stat, int winnerIdx)
    {
        roundResultPanel.SetActive(true);
        var winner = gm.players[winnerIdx];
        bool localWon = winnerIdx == localPlayerIndex;

        if (roundResultText)
            roundResultText.text = localWon ? "🏆 Вы победили в раунде!" : $"Победил {winner.nickname}";
        if (winnerNameText) winnerNameText.text = winner.nickname;
        if (winnerAvatar && winner.avatar) winnerAvatar.sprite = winner.avatar;

        PlaySFX(localWon ? winSfx : loseSfx);
    }

    void HandlePlayerEliminated(int playerIndex)
    {
        StartCoroutine(ShowEliminationMessage(gm.players[playerIndex].nickname));
    }

    IEnumerator ShowEliminationMessage(string name)
    {
        eliminationPanel.SetActive(true);
        if (eliminationText) eliminationText.text = $"{name} выбывает из игры!";
        yield return new WaitForSeconds(2.5f);
        eliminationPanel.SetActive(false);
    }

    void HandleGameOver(int winnerIndex)
    {
        gameOverPanel.SetActive(true);
        bool localWon = winnerIndex == localPlayerIndex;
        if (gameOverWinnerText)
        {
            string name = gm.players[winnerIndex].nickname;
            gameOverWinnerText.text = localWon
                ? $"🎉 ВЫ ПОБЕДИЛИ!\n{name} — чемпион!"
                : $"Победитель: {name}";
        }
        PlaySFX(localWon ? winSfx : loseSfx);
    }

    void UpdateAllCardCounts()
    {
        for (int i = 0; i < playerCardCounts.Length && i < gm.players.Length; i++)
        {
            var p = gm.players[i];
            if (playerCardCounts[i])
                playerCardCounts[i].text = $"Колода: {p.deck.Count}\nРезерв: {p.reserve.Count}";
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip, GameSettings.SFXVolume);
    }

    void PlayAgain()
    {
        PlaySFX(buttonSfx);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void GoToMainMenu()
    {
        PlaySFX(buttonSfx);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
