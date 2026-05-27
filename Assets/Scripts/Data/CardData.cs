using UnityEngine;

[System.Serializable]
public class CardData
{
    public int id;
    public string carName;
    public string carModel;
    public string description;
    public string rarity; // "О" = Обычная, "Р" = Редкая
    public float acceleration; // 0-100 km/h (sec) - LESS IS BETTER
    public int horsepower;     // HP - MORE IS BETTER
    public int maxSpeed;       // km/h - MORE IS BETTER
    public int engineVolume;   // cm³ - MORE IS BETTER
    public int weight;         // kg - LESS IS BETTER
    public int price;          // USD
    public string cardImagePath; // path to sprite resource
    public string[] extraPhotos; // for gallery
}

[System.Serializable]
public class CardsDatabase
{
    public CardData[] cards;
}

public enum StatType
{
    Acceleration,   // LESS IS BETTER
    Horsepower,     // MORE IS BETTER
    MaxSpeed,       // MORE IS BETTER
    EngineVolume,   // MORE IS BETTER
    Weight          // LESS IS BETTER
}

public static class StatTypeExtensions
{
    public static string GetRussianName(this StatType stat)
    {
        switch (stat)
        {
            case StatType.Acceleration: return "0-100 (сек)";
            case StatType.Horsepower:   return "Мощность (л.с.)";
            case StatType.MaxSpeed:     return "Макс. скорость";
            case StatType.EngineVolume: return "Объём двигателя";
            case StatType.Weight:       return "Вес (кг)";
            default: return "";
        }
    }

    public static bool IsLessIsBetter(this StatType stat)
    {
        return stat == StatType.Acceleration || stat == StatType.Weight;
    }

    public static float GetValue(this StatType stat, CardData card)
    {
        switch (stat)
        {
            case StatType.Acceleration:  return card.acceleration;
            case StatType.Horsepower:    return card.horsepower;
            case StatType.MaxSpeed:      return card.maxSpeed;
            case StatType.EngineVolume:  return card.engineVolume;
            case StatType.Weight:        return card.weight;
            default: return 0;
        }
    }
}
