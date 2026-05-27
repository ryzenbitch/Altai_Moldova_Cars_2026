using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GalleryController : MonoBehaviour
{
    [Header("Grid")]
    public Transform    cardGridParent;
    public GameObject   cardThumbnailPrefab;
    public ScrollRect   scrollRect;

    [Header("Detail Panel")]
    public GameObject         detailPanel;
    public Image              detailCarImage;
    public TextMeshProUGUI    detailCarName;
    public TextMeshProUGUI    detailCarModel;
    public TextMeshProUGUI    detailDescription;
    public TextMeshProUGUI    detailRarity;
    public TextMeshProUGUI    detailPrice;
    public TextMeshProUGUI    detailAcceleration;
    public TextMeshProUGUI    detailHorsepower;
    public TextMeshProUGUI    detailMaxSpeed;
    public TextMeshProUGUI    detailEngineVolume;
    public TextMeshProUGUI    detailWeight;
    public Button             prevCardButton;
    public Button             nextCardButton;
    public Button             closeDetailButton;

    [Header("Navigation")]
    public Button backButton;

    List<CardData> allCards;
    int            detailIdx = 0;

    void Start()
    {
        allCards = CardsManager.Instance.GetAllCards();

        if (backButton)        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        if (closeDetailButton) closeDetailButton.onClick.AddListener(() => detailPanel.SetActive(false));
        if (prevCardButton)    prevCardButton.onClick.AddListener(() => Navigate(-1));
        if (nextCardButton)    nextCardButton.onClick.AddListener(() => Navigate(+1));

        if (detailPanel) detailPanel.SetActive(false);
        PopulateGrid();
    }

    void PopulateGrid()
    {
        if (!cardGridParent || !cardThumbnailPrefab) return;
        foreach (Transform c in cardGridParent) Destroy(c.gameObject);

        for (int i = 0; i < allCards.Count; i++)
        {
            int idx  = i;
            var go   = Instantiate(cardThumbnailPrefab, cardGridParent);
            var thumb = go.GetComponent<CardThumbnailUI>();
            if (thumb) thumb.Setup(allCards[i]);

            var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
            btn.onClick.AddListener(() => OpenDetail(idx));
        }
    }

    void OpenDetail(int idx)
    {
        detailIdx = idx;
        RefreshDetail();
        if (detailPanel) detailPanel.SetActive(true);
    }

    void Navigate(int delta)
    {
        detailIdx = Mathf.Clamp(detailIdx + delta, 0, allCards.Count - 1);
        RefreshDetail();
    }

    void RefreshDetail()
    {
        if (detailIdx < 0 || detailIdx >= allCards.Count) return;
        var c = allCards[detailIdx];

        Set(detailCarName,     c.carName);
        Set(detailCarModel,    c.carModel);
        Set(detailDescription, c.description);
        Set(detailRarity,      $"Редкость: {c.rarity}");
        Set(detailPrice,       $"$ {c.price:N0}");

        string accel = c.acceleration >= 99f ? "0-100: —" : $"0-100: {c.acceleration:F1} сек";
        Set(detailAcceleration, accel);
        Set(detailHorsepower,   $"Мощность: {c.horsepower} л.с.");
        Set(detailMaxSpeed,     $"Макс. скорость: {c.maxSpeed} км/ч");
        Set(detailEngineVolume, $"Объём: {c.engineVolume} см³");
        Set(detailWeight,       $"Масса: {c.weight} кг");

        if (detailCarImage)
        {
            var sp = Resources.Load<Sprite>(c.cardImagePath);
            if (sp) detailCarImage.sprite = sp;
        }

        if (prevCardButton) prevCardButton.interactable = (detailIdx > 0);
        if (nextCardButton) nextCardButton.interactable = (detailIdx < allCards.Count - 1);
    }

    static void Set(TextMeshProUGUI t, string v) { if (t) t.text = v; }
}
