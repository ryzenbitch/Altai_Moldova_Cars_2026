using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GameState
{
    Setup,
    DealingCards,
    PlayerTurn,
    SelectingStat,
    RevealingCards,
    ResolvingRound,
    CheckingElimination,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState = GameState.Setup;
    public PlayerData[] players;
    public int currentPlayerIndex = 0;
    public int roundNumber = 0;

    [Header("Current Round")]
    // Cards snapshotted at start of round — NOT nulled until next round
    public CardData[] currentRoundCards;
    public StatType selectedStat;
    public int roundWinnerIndex = -1;

    // Events
    public System.Action<GameState>               OnStateChanged;
    public System.Action<int>                     OnTurnChanged;        // playerIndex
    public System.Action<CardData[], StatType, int> OnRoundResolved;    // snapshot, stat, winner
    public System.Action<int>                     OnPlayerEliminated;
    public System.Action<int>                     OnGameOver;           // winner index
    public System.Action                          OnCardsDealt;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    // ── Public API ────────────────────────────────────────────────────────
    public void StartGame()
    {
        if (GameSettings.Players == null || GameSettings.Players.Length == 0)
        {
            Debug.LogError("[GameManager] No players configured — call GameSettings.SetupSoloBotGame() first!");
            GameSettings.SetupSoloBotGame();
        }

        players = GameSettings.Players;
        currentPlayerIndex = 0;
        roundNumber = 0;
        currentRoundCards = new CardData[players.Length];

        // Distribute 6 cards each
        var gameDeck = CardsManager.Instance.CreateGameDeck(players.Length);
        const int cardsPerPlayer = 6;
        for (int i = 0; i < players.Length; i++)
        {
            players[i].deck.Clear();
            players[i].reserve.Clear();
            for (int j = 0; j < cardsPerPlayer && gameDeck.Count > 0; j++)
            {
                players[i].deck.Add(gameDeck[0]);
                gameDeck.RemoveAt(0);
            }
        }

        currentPlayerIndex = Random.Range(0, players.Length);
        // Make sure first player is not eliminated (safety)
        while (!players[currentPlayerIndex].HasCards())
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

        OnCardsDealt?.Invoke();
        SetState(GameState.PlayerTurn);
        OnTurnChanged?.Invoke(currentPlayerIndex);
    }

    /// <summary>UI calls this once the current player has "drawn" their card.</summary>
    public void PlayerReadyToSelectStat()
    {
        if (currentState != GameState.PlayerTurn) return;

        currentRoundCards[currentPlayerIndex] = players[currentPlayerIndex].DrawTopCard();
        SetState(GameState.SelectingStat);

        if (players[currentPlayerIndex].playerType == PlayerType.Bot)
            StartCoroutine(BotSelectStat());
    }

    /// <summary>UI calls this when the human player picks a stat.</summary>
    public void SelectStat(StatType stat)
    {
        if (currentState != GameState.SelectingStat) return;
        selectedStat = stat;
        SetState(GameState.RevealingCards);
        StartCoroutine(RevealAndResolve());
    }

    // ── Internal flow ─────────────────────────────────────────────────────
    void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    IEnumerator BotSelectStat()
    {
        yield return new WaitForSeconds(1.4f);
        SelectStat(BotAI.ChooseBestStat(currentRoundCards[currentPlayerIndex]));
    }

    IEnumerator RevealAndResolve()
    {
        // Draw opponent cards
        for (int i = 0; i < players.Length; i++)
        {
            if (i == currentPlayerIndex) continue;
            if (players[i].IsEliminated()) continue;
            currentRoundCards[i] = players[i].DrawTopCard();
        }

        yield return new WaitForSeconds(0.4f);
        SetState(GameState.ResolvingRound);

        roundWinnerIndex = DetermineWinner(selectedStat);

        // ── SNAPSHOT before nulling ──
        var snapshot = (CardData[])currentRoundCards.Clone();

        // Move cards to winner's reserve
        for (int i = 0; i < players.Length; i++)
        {
            if (currentRoundCards[i] != null)
            {
                players[roundWinnerIndex].reserve.Add(currentRoundCards[i]);
                currentRoundCards[i] = null;
            }
        }

        // Fire event with snapshot (not the now-null array)
        OnRoundResolved?.Invoke(snapshot, selectedStat, roundWinnerIndex);

        yield return new WaitForSeconds(2.2f);

        SetState(GameState.CheckingElimination);
        CheckEliminations();
    }

    int DetermineWinner(StatType stat)
    {
        int bestIdx   = -1;
        float bestVal = stat.IsLessIsBetter() ? float.MaxValue : float.MinValue;

        for (int i = 0; i < players.Length; i++)
        {
            if (currentRoundCards[i] == null) continue;

            float val     = stat.GetValue(currentRoundCards[i]);
            bool  better  = stat.IsLessIsBetter() ? val < bestVal : val > bestVal;

            if (bestIdx == -1 || better) { bestVal = val; bestIdx = i; }
        }
        return bestIdx >= 0 ? bestIdx : currentPlayerIndex;
    }

    void CheckEliminations()
    {
        // Notify UI about newly-eliminated players
        for (int i = 0; i < players.Length; i++)
            if (!players[i].HasCards())
                OnPlayerEliminated?.Invoke(i);

        var active = players.Where(p => p.HasCards()).ToList();

        if (active.Count <= 1)
        {
            int winnerIdx = active.Count == 1
                ? System.Array.IndexOf(players, active[0])
                : roundWinnerIndex;
            SetState(GameState.GameOver);
            OnGameOver?.Invoke(winnerIdx);
            return;
        }

        // Advance turn clockwise, skip eliminated
        roundNumber++;
        int next = (currentPlayerIndex + 1) % players.Length;
        for (int guard = 0; guard < players.Length; guard++)
        {
            if (players[next].HasCards()) break;
            next = (next + 1) % players.Length;
        }
        currentPlayerIndex = next;

        // Reset round card slots
        for (int i = 0; i < currentRoundCards.Length; i++)
            currentRoundCards[i] = null;

        SetState(GameState.PlayerTurn);
        OnTurnChanged?.Invoke(currentPlayerIndex);
    }
}

// ── Bot AI ────────────────────────────────────────────────────────────────
public static class BotAI
{
    public static StatType ChooseBestStat(CardData card)
    {
        float accelScore  = Mathf.Clamp(100f - card.acceleration / 20f * 100f, 0f, 100f);
        float hpScore     = Mathf.Clamp(card.horsepower / 800f * 100f,  0f, 100f);
        float speedScore  = Mathf.Clamp(card.maxSpeed   / 350f * 100f,  0f, 100f);
        float volScore    = Mathf.Clamp(card.engineVolume / 8000f * 100f, 0f, 100f);
        float weightScore = Mathf.Clamp(100f - card.weight / 2500f * 100f, 0f, 100f);

        var scores = new Dictionary<StatType, float>
        {
            { StatType.Acceleration,  accelScore  },
            { StatType.Horsepower,    hpScore     },
            { StatType.MaxSpeed,      speedScore  },
            { StatType.EngineVolume,  volScore    },
            { StatType.Weight,        weightScore }
        };

        StatType best      = StatType.Horsepower;
        float    bestScore = -1f;
        foreach (var kv in scores)
        {
            float score = kv.Value + Random.Range(0f, 18f); // slight randomness
            if (score > bestScore) { bestScore = score; best = kv.Key; }
        }
        return best;
    }
}
