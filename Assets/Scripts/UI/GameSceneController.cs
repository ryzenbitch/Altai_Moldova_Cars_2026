using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Master controller for the Game scene.
/// Wires all GameManager events to UI panels.
/// Attach to [GameSceneController] GameObject in GameScene.
/// </summary>
public class GameSceneController : MonoBehaviour
{
    [Header("Player Slots (0=Opp1, 1=Opp2, 2=LocalPlayer)")]
    public PlayerSlotUI[] playerSlots;   // length 3
    public DeckCounterUI[] deckCounters; // length 3

    [Header("Card Displays")]
    public CardDisplayUI myCardDisplay;
    public CardDisplayUI[] opponentCardDisplays; // length 2

    [Header("Stat Panel")]
    public GameObject statPanel;
    public StatButtonUI[] statButtons; // 5 buttons

    [Header("Result & Notices")]
    public RoundResultPanel roundResultPanel;
    public GameObject eliminationNotice;
    public TextMeshProUGUI eliminationText;

    [Header("HUD")]
    public TextMeshProUGUI turnBannerText;
    public GameObject myTurnBanner;
    public TextMeshProUGUI roundCounterText;

    [Header("Game Over")]
    public GameOverScreen gameOverScreen;

    // ── Private ───────────────────────────────────────────────────────────
    GameManager gm;
    int localIdx = 0; // local human is always players[0]

    // ── Unity ─────────────────────────────────────────────────────────────
    void Start()
    {
        // If no game settings exist (e.g. scene opened directly), default to vs bot
        if (GameSettings.Players == null || GameSettings.Players.Length == 0)
            GameSettings.SetupSoloBotGame();

        gm = GameManager.Instance;
        if (gm == null)
        {
            // GameManager wasn't in scene — create one
            gm = new GameObject("[GameManager]").AddComponent<GameManager>();
        }

        Subscribe();
        InitUI();
        gm.StartGame();
    }

    void OnDestroy() => Unsubscribe();

    // ── Subscription ──────────────────────────────────────────────────────
    void Subscribe()
    {
        gm.OnCardsDealt += HandleCardsDealt;
        gm.OnStateChanged += HandleStateChanged;
        gm.OnTurnChanged += HandleTurnChanged;
        gm.OnRoundResolved += HandleRoundResolved;
        gm.OnPlayerEliminated += HandlePlayerEliminated;
        gm.OnGameOver += HandleGameOver;
    }

    void Unsubscribe()
    {
        if (gm == null) return;
        gm.OnCardsDealt -= HandleCardsDealt;
        gm.OnStateChanged -= HandleStateChanged;
        gm.OnTurnChanged -= HandleTurnChanged;
        gm.OnRoundResolved -= HandleRoundResolved;
        gm.OnPlayerEliminated -= HandlePlayerEliminated;
        gm.OnGameOver -= HandleGameOver;
    }

    // ── Init ──────────────────────────────────────────────────────────────
    void InitUI()
    {
        statPanel.SetActive(false);
        if (myTurnBanner) myTurnBanner.SetActive(false);
        if (eliminationNotice) eliminationNotice.SetActive(false);
        if (gameOverScreen) gameOverScreen.gameObject.SetActive(false);

        // Setup player slot UIs
        // Slot layout: slots[0]=opponent1 (top-left), slots[1]=opponent2 (top-right),
        //              slots[2]=local player (bottom)
        // gm.players[0] = local human, gm.players[1..] = others
        // So: slot[2] shows players[0], slot[0] shows players[1], slot[1] shows players[2]
        for (int slotIdx = 0; slotIdx < playerSlots.Length; slotIdx++)
        {
            int playerIdx = SlotToPlayerIndex(slotIdx);
            bool exists = playerIdx < gm.players.Length;
            playerSlots[slotIdx].gameObject.SetActive(exists);
            if (exists) playerSlots[slotIdx].Setup(gm.players[playerIdx]);
        }

        // Hide second opponent slot if only 2 players
        if (gm.players.Length < 3 && playerSlots.Length > 1)
            playerSlots[1].gameObject.SetActive(false);
        if (gm.players.Length < 3 && opponentCardDisplays.Length > 1)
            opponentCardDisplays[1].gameObject.SetActive(false);

        HideAllCardDisplays();
        RefreshAllCounters();
    }

    // Mapping: UI slot 0 → player 1, slot 1 → player 2, slot 2 → player 0 (local)
    int SlotToPlayerIndex(int slot) => slot == 2 ? 0 : slot + 1;
    int PlayerToSlot(int playerIdx) => playerIdx == 0 ? 2 : playerIdx - 1;

    // ── Event Handlers ────────────────────────────────────────────────────
    void HandleCardsDealt()
    {
        RefreshAllCounters();
        if (roundCounterText) roundCounterText.text = "Раунд 1";
    }

    void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.PlayerTurn:
                HideAllCardDisplays();
                statPanel.SetActive(false);
                if (myTurnBanner) myTurnBanner.SetActive(false);
                RefreshAllCounters();
                RefreshTurnHighlights();
                if (roundCounterText) roundCounterText.text = $"Раунд {gm.roundNumber + 1}";

                bool isMyTurn = (gm.currentPlayerIndex == localIdx);
                if (myTurnBanner) myTurnBanner.SetActive(isMyTurn);

                if (isMyTurn)
                    StartCoroutine(DoLocalPlayerTurn());
                else
                    StartCoroutine(DoBotTurnDraw());
                break;

            case GameState.RevealingCards:
                statPanel.SetActive(false);
                RevealOpponentCards();
                break;
        }
    }

    IEnumerator DoLocalPlayerTurn()
    {
        yield return new WaitForSeconds(0.35f);
        if (AudioManager.Instance) AudioManager.Instance.PlayCardDeal();
        gm.PlayerReadyToSelectStat();
        // Show card and stat panel
        if (gm.currentRoundCards[localIdx] != null)
            myCardDisplay.ShowCard(gm.currentRoundCards[localIdx]);
        ShowStatPanel(gm.currentRoundCards[localIdx]);
    }

    IEnumerator DoBotTurnDraw()
    {
        yield return new WaitForSeconds(0.55f);
        if (AudioManager.Instance) AudioManager.Instance.PlayCardDeal();
        gm.PlayerReadyToSelectStat();
        // Show bot's card face-down (back) in opponent slot
        int opSlot = PlayerToSlot(gm.currentPlayerIndex) - 1; // opponent slots are 0 & 1
        if (opSlot >= 0 && opSlot < opponentCardDisplays.Length)
            opponentCardDisplays[opSlot].gameObject.SetActive(true);
    }

    void ShowStatPanel(CardData card)
    {
        if (card == null) return;
        statPanel.SetActive(true);

        StatType[] types = {
            StatType.Acceleration, StatType.Horsepower,
            StatType.MaxSpeed, StatType.EngineVolume, StatType.Weight
        };
        for (int i = 0; i < statButtons.Length && i < types.Length; i++)
        {
            StatType t = types[i];
            statButtons[i].Setup(t, t.GetValue(card), chosen => {
                if (AudioManager.Instance) AudioManager.Instance.PlayCardSlide();
                statPanel.SetActive(false);
                gm.SelectStat(chosen);
            });
        }
    }

    void RevealOpponentCards()
    {
        for (int pi = 0; pi < gm.players.Length; pi++)
        {
            if (pi == localIdx) continue;
            var card = gm.currentRoundCards[pi];
            if (card == null) continue;

            int opSlot = pi - 1; // players[1]→slot0, players[2]→slot1
            if (opSlot >= 0 && opSlot < opponentCardDisplays.Length)
            {
                if (AudioManager.Instance) AudioManager.Instance.PlayCardFlip();
                opponentCardDisplays[opSlot].ShowCard(card);
            }
        }
    }

    void HandleTurnChanged(int playerIdx)
    {
        RefreshTurnHighlights();
        string name = gm.players[playerIdx].nickname;
        if (turnBannerText) turnBannerText.text = $"Ходит: {name}";
    }

    void HandleRoundResolved(CardData[] snapshot, StatType stat, int winnerIdx)
    {
        RefreshAllCounters();

        // Highlight stats on visible cards
        for (int pi = 0; pi < snapshot.Length; pi++)
        {
            if (snapshot[pi] == null) continue;
            bool won = (pi == winnerIdx);
            if (pi == localIdx)
                myCardDisplay.HighlightStat(stat, won);
            else
            {
                int opSlot = pi - 1;
                if (opSlot >= 0 && opSlot < opponentCardDisplays.Length)
                    opponentCardDisplays[opSlot].HighlightStat(stat, won);
            }
        }

        // Show result panel.
        // IMPORTANT: auto-hide coroutine is started HERE (GameSceneController is
        // always active) instead of inside RoundResultPanel, which avoids the
        // "Coroutine couldn't be started because the game object is inactive" error
        // that triggers when the panel or its parent is not yet active in the hierarchy.
        roundResultPanel.ShowResult(gm.players, snapshot, stat, winnerIdx);
        if (roundResultPanel.autoHideDelay > 0f)
            StartCoroutine(AutoHideRoundResult(roundResultPanel.autoHideDelay));

        bool localWon = (winnerIdx == localIdx);
        if (AudioManager.Instance)
        {
            if (localWon) AudioManager.Instance.PlayWinRound();
            else AudioManager.Instance.PlayLoseRound();
        }
    }

    // Driven from GameSceneController (always active) so the coroutine never fails
    // due to the panel being inactive at start time.
    IEnumerator AutoHideRoundResult(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (roundResultPanel) roundResultPanel.Hide();
    }

    void HandlePlayerEliminated(int playerIdx)
    {
        int slot = PlayerToSlot(playerIdx);
        if (slot < playerSlots.Length) playerSlots[slot].ShowEliminated();
        if (AudioManager.Instance) AudioManager.Instance.PlayEliminated();
        StartCoroutine(ShowEliminationNotice(gm.players[playerIdx].nickname));
    }

    IEnumerator ShowEliminationNotice(string playerName)
    {
        if (eliminationNotice) eliminationNotice.SetActive(true);
        if (eliminationText) eliminationText.text = $"{playerName} выбывает из игры!";
        yield return new WaitForSeconds(2.5f);
        if (eliminationNotice) eliminationNotice.SetActive(false);
    }

    void HandleGameOver(int winnerIdx)
    {
        if (gameOverScreen)
            gameOverScreen.Show(
                gm.players[winnerIdx],
                gm.players,
                gm.roundNumber,
                winnerIdx == localIdx
            );
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    void RefreshAllCounters()
    {
        for (int slotIdx = 0; slotIdx < deckCounters.Length; slotIdx++)
        {
            if (deckCounters[slotIdx] == null) continue;
            int pi = SlotToPlayerIndex(slotIdx);
            if (pi < gm.players.Length)
                deckCounters[slotIdx].UpdateCounts(
                    gm.players[pi].deck.Count,
                    gm.players[pi].reserve.Count);
        }
    }

    void RefreshTurnHighlights()
    {
        for (int slotIdx = 0; slotIdx < playerSlots.Length; slotIdx++)
        {
            if (playerSlots[slotIdx] == null) continue;
            int pi = SlotToPlayerIndex(slotIdx);
            playerSlots[slotIdx].SetActiveTurn(pi == gm.currentPlayerIndex);
        }
    }

    void HideAllCardDisplays()
    {
        myCardDisplay.HideCard();
        foreach (var d in opponentCardDisplays) if (d) d.HideCard();
    }
}
