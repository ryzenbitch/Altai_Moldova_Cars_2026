using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Attach to the CardThumbnail prefab.</summary>
public class CardThumbnailUI : MonoBehaviour
{
    public Image             cardImage;
    public TextMeshProUGUI   nameLabel;
    public TextMeshProUGUI   rarityBadge;
    public GameObject        glowEffect; // optional rare glow

    public void Setup(CardData card)
    {
        if (nameLabel)    nameLabel.text    = card.carName;
        if (rarityBadge)  rarityBadge.text  = card.rarity;
        if (glowEffect)   glowEffect.SetActive(card.rarity == "Р");

        if (cardImage)
        {
            var sp = Resources.Load<Sprite>(card.cardImagePath);
            if (sp) cardImage.sprite = sp;
        }
    }
}
