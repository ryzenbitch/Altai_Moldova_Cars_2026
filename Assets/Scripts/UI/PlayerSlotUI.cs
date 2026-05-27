using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    [Header("Info")]
    public Image             avatarImage;
    public Sprite            defaultAvatar;
    public TextMeshProUGUI   nicknameText;

    [Header("Turn glow")]
    public GameObject activeTurnGlow;
    public Image      borderImage;
    public Color      activeColor   = new Color(1f, 0.84f, 0f, 1f);
    public Color      inactiveColor = new Color(0.3f, 0.4f, 0.3f, 1f);

    [Header("Eliminated")]
    public GameObject eliminatedOverlay;

    PlayerData player;

    public void Setup(PlayerData p)
    {
        player = p;
        if (nicknameText) nicknameText.text = p.nickname;
        if (avatarImage)  avatarImage.sprite = p.avatar ?? defaultAvatar;
        if (eliminatedOverlay) eliminatedOverlay.SetActive(false);
        SetActiveTurn(false);
    }

    public void SetActiveTurn(bool active)
    {
        if (activeTurnGlow) activeTurnGlow.SetActive(active);
        if (borderImage)    borderImage.color = active ? activeColor : inactiveColor;
    }

    public void ShowEliminated()
    {
        if (eliminatedOverlay) eliminatedOverlay.SetActive(true);
        SetActiveTurn(false);
    }
}
