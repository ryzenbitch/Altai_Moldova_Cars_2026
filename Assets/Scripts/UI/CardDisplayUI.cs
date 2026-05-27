using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a single card. Attach to each card UI root object.
/// Call ShowCard() / HideCard() to control visibility.
/// </summary>
public class CardDisplayUI : MonoBehaviour
{
    [Header("Sides")]
    public GameObject cardFront;
    public GameObject cardBack;

    [Header("Car Image")]
    public Image carImage;

    [Header("Header badges")]
    public TextMeshProUGUI cardIdText;
    public TextMeshProUGUI rarityText;

    [Header("Name / Description")]
    public TextMeshProUGUI carNameText;
    public TextMeshProUGUI carModelText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;

    [Header("Stats (order: Accel, HP, Speed, Volume, Weight)")]
    public TextMeshProUGUI accelerationText;
    public TextMeshProUGUI horsepowerText;
    public TextMeshProUGUI maxSpeedText;
    public TextMeshProUGUI engineVolumeText;
    public TextMeshProUGUI weightText;

    [Header("Highlight overlays (same order as stats)")]
    public Image[] statHighlights; // length 5

    [Header("Highlight colours")]
    public Color winColor  = new Color(0.1f, 0.9f, 0.1f, 0.55f);
    public Color loseColor = new Color(0.9f, 0.1f, 0.1f, 0.35f);

    CardData currentCard;

    // ── Unity ─────────────────────────────────────────────────────────────
    void Awake() => HideCard();

    // ── Public API ────────────────────────────────────────────────────────
    public void ShowCard(CardData card, bool animate = true)
    {
        if (card == null) return;
        currentCard = card;

        Populate(card);
        if (cardBack)  cardBack.SetActive(false);
        if (cardFront) cardFront.SetActive(true);

        if (animate)
        {
            var flip = GetComponent<CardFlipAnimation>();
            if (flip) flip.FlipToFront();
        }
    }

    public void HideCard()
    {
        currentCard = null;
        ClearStatHighlights();
        if (cardFront) cardFront.SetActive(false);
        if (cardBack)  cardBack.SetActive(true);
    }

    public void HighlightStat(StatType stat, bool isWinner)
    {
        ClearStatHighlights();
        int idx = (int)stat;
        if (statHighlights != null && idx < statHighlights.Length && statHighlights[idx] != null)
            statHighlights[idx].color = isWinner ? winColor : loseColor;
    }

    public void ClearStatHighlights()
    {
        if (statHighlights == null) return;
        foreach (var h in statHighlights)
            if (h) h.color = Color.clear;
    }

    public CardData GetCard() => currentCard;

    // ── Private ───────────────────────────────────────────────────────────
    void Populate(CardData card)
    {
        Set(cardIdText,       card.id.ToString());
        Set(rarityText,       card.rarity);
        Set(carNameText,      card.carName);
        Set(carModelText,     card.carModel);
        Set(descriptionText,  card.description);
        Set(priceText,        $"$ {card.price:N0}");

        Set(accelerationText, card.acceleration >= 99f ? "—" : $"{card.acceleration:F1}");
        Set(horsepowerText,   card.horsepower.ToString());
        Set(maxSpeedText,     card.maxSpeed.ToString());
        Set(engineVolumeText, card.engineVolume.ToString());
        Set(weightText,       card.weight.ToString());

        if (carImage)
        {
            var sprite = Resources.Load<Sprite>(card.cardImagePath);
            if (sprite) carImage.sprite = sprite;
        }
    }

    static void Set(TextMeshProUGUI t, string v) { if (t) t.text = v; }
}
