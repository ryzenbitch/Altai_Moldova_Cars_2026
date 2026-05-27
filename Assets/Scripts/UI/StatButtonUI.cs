using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// One stat-selection button. Call Setup() each turn.
/// </summary>
public class StatButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Labels")]
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;

    [Header("Visuals")]
    public Image background;
    public Color normalColor   = new Color(0.15f, 0.15f, 0.25f, 0.95f);
    public Color hoverColor    = new Color(0.25f, 0.45f, 0.85f, 1.00f);
    public Color selectedColor = new Color(1.00f, 0.75f, 0.00f, 1.00f);

    Button button;
    bool   selected;

    void Awake()
    {
        button = GetComponent<Button>();
        if (!background) background = GetComponent<Image>();
        if (background) background.color = normalColor;
    }

    /// <param name="onSelect">Callback fired when this button is clicked.</param>
    public void Setup(StatType statType, float value, System.Action<StatType> onSelect)
    {
        selected = false;
        if (background) background.color = normalColor;

        // Name
        if (statNameText) statNameText.text = statType.GetRussianName();

        // Value + hint
        string unit = statType switch
        {
            StatType.Acceleration  => " сек",
            StatType.MaxSpeed      => " км/ч",
            StatType.EngineVolume  => " см³",
            StatType.Weight        => " кг",
            _                      => " л.с."
        };
        string display = (statType == StatType.Acceleration && value >= 99f) ? "—" : $"{value:F0}{unit}";
        string hint    = statType.IsLessIsBetter() ? "↓ меньше лучше" : "↑ больше лучше";

        if (statValueText) statValueText.text = $"{display}\n<size=70%><color=#aaaaaa>{hint}</color></size>";

        // Hook click
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { Select(); onSelect?.Invoke(statType); });
        }
    }

    public void Select()
    {
        selected = true;
        if (background) background.color = selectedColor;
    }

    public void Deselect()
    {
        selected = false;
        if (background) background.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (!selected && background) background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (!selected && background) background.color = normalColor;
    }
}
