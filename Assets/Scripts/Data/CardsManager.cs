using UnityEngine;
using System.Collections.Generic;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance { get; private set; }

    private List<CardData> allCards = new List<CardData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        InitializeCards();
    }

    void InitializeCards()
    {
        allCards = new List<CardData>
        {
            new CardData {
                id = 7, carName = "SUBARU BRZ", carModel = "ZD8 2.4 S",
                description = "Несмотря на то, что BRZ — это спортивное купе, её практичность удивительно хороша. Лёгкий заднеприводный спорткар с отличной управляемостью.",
                rarity = "Р",
                acceleration = 6.8f, horsepower = 235, maxSpeed = 226, engineVolume = 2387, weight = 1290,
                price = 35000, cardImagePath = "Cards/7"
            },
            new CardData {
                id = 8, carName = "SKODA SUPERB", carModel = "3 Restyling 2.0 TSI DSG SportLine",
                description = "Название Superb («превосходный») впервые использовали еще в 1934 году для лимузинов Skoda. Флагман чешского производителя с богатым оснащением.",
                rarity = "О",
                acceleration = 7.0f, horsepower = 220, maxSpeed = 247, engineVolume = 1984, weight = 1510,
                price = 40000, cardImagePath = "Cards/8"
            },
            new CardData {
                id = 9, carName = "JEEP GRAND CHEROKEE", carModel = "WJ (MK2), 4.0AT 4x4",
                description = "Это последний «настоящий» мостовой Grand Cherokee, сочетающий внедорожные с комфортом. Легенда американского бездорожья.",
                rarity = "О",
                acceleration = 10.9f, horsepower = 190, maxSpeed = 189, engineVolume = 3964, weight = 1860,
                price = 8500, cardImagePath = "Cards/9"
            },
            new CardData {
                id = 10, carName = "LADA 2106", carModel = "1.6 МТ5",
                description = "«Шестёрка» — легендарный советский седан, выпускавшийся с 1976 по 2006 года. Культовый автомобиль советской эпохи, надёжный и простой в обслуживании.",
                rarity = "О",
                acceleration = 16.0f, horsepower = 75, maxSpeed = 155, engineVolume = 1569, weight = 1050,
                price = 3400, cardImagePath = "Cards/10"
            },
            new CardData {
                id = 11, carName = "ASTON MARTIN VALOUR", carModel = "5.2 litre",
                description = "Кузов Valour изготовлен из смеси алюминия и карбона, что повышает его манёвренность. Эксклюзивный британский суперкар в честь 110-летия марки.",
                rarity = "О",
                acceleration = 3.5f, horsepower = 715, maxSpeed = 322, engineVolume = 5204, weight = 1850,
                price = 1500000, cardImagePath = "Cards/11"
            },
            new CardData {
                id = 12, carName = "FORD GT40", carModel = "Mk II Daytona",
                description = "В 1966-1969-х гоночная версия этого авто выигрывала гонку «24 часа Ле-Мана». Легендарный американский гоночный автомобиль, победитель Ле-Мана.",
                rarity = "О",
                acceleration = 4.3f, horsepower = 485, maxSpeed = 330, engineVolume = 6997, weight = 1048,
                price = 9800000, cardImagePath = "Cards/12"
            },
            new CardData {
                id = 13, carName = "ЗАЗ 965", carModel = "0,9 МТ",
                description = "После фильма «Место встречи изменить нельзя» за машиной закрепилось прозвище «горбатый». Народный советский микролитражный автомобиль.",
                rarity = "О",
                acceleration = 99f, horsepower = 27, maxSpeed = 90, engineVolume = 887, weight = 650,
                price = 4000, cardImagePath = "Cards/13"
            }
        };
    }

    public List<CardData> GetAllCards() => new List<CardData>(allCards);

    public CardData GetCardById(int id) => allCards.Find(c => c.id == id);

    public List<CardData> GetShuffledDeck()
    {
        var deck = GetAllCards();
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
        return deck;
    }

    // Creates a deck of 6*playerCount cards (with repetition if needed)
    public List<CardData> CreateGameDeck(int playerCount)
    {
        int totalCards = 6 * playerCount;
        var shuffled = GetShuffledDeck();
        var result = new List<CardData>();
        while (result.Count < totalCards)
        {
            foreach (var card in shuffled)
            {
                result.Add(card);
                if (result.Count >= totalCards) break;
            }
        }
        // Final shuffle
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }
}
