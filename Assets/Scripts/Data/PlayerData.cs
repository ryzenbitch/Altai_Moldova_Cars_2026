using UnityEngine;
using System.Collections.Generic;

public enum PlayerType { Human, Bot }

[System.Serializable]
public class PlayerData
{
    public string     nickname;
    public Sprite     avatar;
    public PlayerType playerType;
    public int        seatIndex;

    public List<CardData> deck    = new List<CardData>();
    public List<CardData> reserve = new List<CardData>();

    public int TotalCards => deck.Count + reserve.Count;

    public CardData DrawTopCard()
    {
        // Refill deck from reserve if empty
        if (deck.Count == 0)
        {
            if (reserve.Count == 0) return null;
            deck = new List<CardData>(reserve);
            reserve.Clear();
            Shuffle(deck);
        }
        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public bool HasCards() => deck.Count > 0 || reserve.Count > 0;
    public bool IsEliminated() => !HasCards();

    static void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

/// <summary>Global static game settings shared between scenes.</summary>
public static class GameSettings
{
    public static int        PlayerCount      = 2;
    public static PlayerData[] Players;
    public static int        HumanPlayerCount = 1;

    public static string     Language         = "RU";
    public static float      MusicVolume      = 0.7f;
    public static float      SFXVolume        = 1.0f;
    public static string     LocalNickname    = "Игрок";
    public static Sprite     LocalAvatar      = null;

    public static void SetupSoloBotGame()
    {
        PlayerCount      = 2;
        HumanPlayerCount = 1;
        Players = new PlayerData[2]
        {
            new PlayerData { nickname = LocalNickname, avatar = LocalAvatar,
                             playerType = PlayerType.Human, seatIndex = 0 },
            new PlayerData { nickname = "GONKI-BOT",
                             playerType = PlayerType.Bot,   seatIndex = 1 }
        };
    }

    public static void SetupLocalMultiplayer(string[] names, Sprite[] avatars, int humanCount)
    {
        PlayerCount      = names.Length;
        HumanPlayerCount = humanCount;
        Players          = new PlayerData[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            Players[i] = new PlayerData
            {
                nickname   = names[i],
                avatar     = (avatars != null && i < avatars.Length) ? avatars[i] : null,
                playerType = (i < humanCount) ? PlayerType.Human : PlayerType.Bot,
                seatIndex  = i
            };
        }
    }
}
