using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple avatar picker panel with preset avatar sprites.
/// </summary>
public class AvatarPickerUI : MonoBehaviour
{
    [Header("Preset Avatars")]
    public Sprite[] presetAvatars;
    public Transform avatarGrid;
    public GameObject avatarButtonPrefab;

    [Header("Result")]
    public Image previewImage;
    public Button confirmButton;
    public Button closeButton;

    private System.Action<Sprite> onAvatarSelected;
    private Sprite currentSelection;

    void Start()
    {
        confirmButton.onClick.AddListener(Confirm);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        PopulateGrid();
    }

    public void Open(System.Action<Sprite> callback, Sprite currentAvatar = null)
    {
        gameObject.SetActive(true);
        onAvatarSelected = callback;
        currentSelection = currentAvatar;
        if (previewImage && currentAvatar) previewImage.sprite = currentAvatar;
    }

    void PopulateGrid()
    {
        foreach (Transform child in avatarGrid) Destroy(child.gameObject);

        foreach (var sprite in presetAvatars)
        {
            var btn = Instantiate(avatarButtonPrefab, avatarGrid);
            var img = btn.GetComponentInChildren<Image>();
            if (img) img.sprite = sprite;

            Sprite captured = sprite;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectAvatar(captured));
        }
    }

    void SelectAvatar(Sprite sprite)
    {
        currentSelection = sprite;
        if (previewImage) previewImage.sprite = sprite;
    }

    void Confirm()
    {
        if (currentSelection != null)
            onAvatarSelected?.Invoke(currentSelection);
        gameObject.SetActive(false);
    }
}
