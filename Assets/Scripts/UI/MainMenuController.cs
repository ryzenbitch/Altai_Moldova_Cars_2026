using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button playWithBotButton;
    public Button playWithFriendButton;
    public Button galleryButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Settings Panel")]
    public GameObject    settingsPanel;
    public Slider        musicSlider;
    public Slider        sfxSlider;
    public TMP_InputField nicknameInput;
    public Button        avatarButton;
    public Image         selectedAvatarImage;
    public Button        langRuButton;
    public Button        langEnButton;
    public Button        closeSettingsButton;

    [Header("Multiplayer Setup Panel")]
    public GameObject     multiplayerSetupPanel;
    public Button         twoPlayersButton;
    public Button         threePlayersButton;
    public TMP_InputField[] playerNameInputs;   // [0..2]
    public Button[]          playerAvatarButtons; // [0..2]
    public Button         startMultiButton;
    public Button         closeMultiButton;

    [Header("Audio")]
    public AudioSource bgMusic;
    public AudioSource sfxSource;
    public AudioClip   buttonSfx;
    public AudioClip   cardShuffleSfx;

    int selectedPlayerCount = 2;

    // ── Unity ─────────────────────────────────────────────────────────────
    void Start()
    {
        LoadSettings();

        // Main buttons
        playWithBotButton   .onClick.AddListener(PlayWithBot);
        playWithFriendButton.onClick.AddListener(OpenMultiplayer);
        galleryButton       .onClick.AddListener(OpenGallery);
        settingsButton      .onClick.AddListener(OpenSettings);
        exitButton          .onClick.AddListener(QuitGame);

        // Settings
        if (closeSettingsButton) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (langRuButton) langRuButton.onClick.AddListener(() => SetLang("RU"));
        if (langEnButton) langEnButton.onClick.AddListener(() => SetLang("EN"));

        // Multiplayer
        if (closeMultiButton)    closeMultiButton.onClick.AddListener(CloseMultiplayer);
        if (twoPlayersButton)    twoPlayersButton.onClick.AddListener(() => SelectCount(2));
        if (threePlayersButton)  threePlayersButton.onClick.AddListener(() => SelectCount(3));
        if (startMultiButton)    startMultiButton.onClick.AddListener(StartMultiplayer);

        // Panels default hidden
        if (settingsPanel)         settingsPanel.SetActive(false);
        if (multiplayerSetupPanel) multiplayerSetupPanel.SetActive(false);

        // Apply music
        if (bgMusic) { bgMusic.volume = GameSettings.MusicVolume; bgMusic.Play(); }
    }

    // ── Main Button Handlers ──────────────────────────────────────────────
    void PlayWithBot()
    {
        PlaySFX(cardShuffleSfx);
        GameSettings.SetupSoloBotGame();
        SceneManager.LoadScene("GameScene");
    }

    void OpenMultiplayer()
    {
        PlaySFX(buttonSfx);
        if (multiplayerSetupPanel) multiplayerSetupPanel.SetActive(true);
        SelectCount(2);
    }

    void OpenGallery()
    {
        PlaySFX(buttonSfx);
        SceneManager.LoadScene("GalleryScene");
    }

    void OpenSettings()
    {
        PlaySFX(buttonSfx);
        if (settingsPanel) settingsPanel.SetActive(true);

        if (musicSlider)   { musicSlider.value = GameSettings.MusicVolume; }
        if (sfxSlider)     { sfxSlider.value   = GameSettings.SFXVolume; }
        if (nicknameInput) { nicknameInput.text = GameSettings.LocalNickname; }

        if (musicSlider) musicSlider.onValueChanged.AddListener(v =>
        {
            GameSettings.MusicVolume = v;
            if (bgMusic) bgMusic.volume = v;
            PlayerPrefs.SetFloat("MusicVolume", v);
        });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(v =>
        {
            GameSettings.SFXVolume = v;
            PlayerPrefs.SetFloat("SFXVolume", v);
        });
        if (nicknameInput) nicknameInput.onEndEdit.AddListener(n =>
        {
            GameSettings.LocalNickname = string.IsNullOrWhiteSpace(n) ? "Игрок" : n;
            PlayerPrefs.SetString("Nickname", GameSettings.LocalNickname);
        });
    }

    void CloseSettings()
    {
        PlaySFX(buttonSfx);
        if (musicSlider)   musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider)     sfxSlider.onValueChanged.RemoveAllListeners();
        if (nicknameInput) nicknameInput.onEndEdit.RemoveAllListeners();
        if (settingsPanel) settingsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Multiplayer Setup ─────────────────────────────────────────────────
    void SelectCount(int count)
    {
        selectedPlayerCount = count;
        if (twoPlayersButton)   twoPlayersButton.interactable   = (count != 2);
        if (threePlayersButton) threePlayersButton.interactable = (count != 3);

        for (int i = 0; i < 3; i++)
        {
            bool show = i < count;
            if (i < playerNameInputs.Length && playerNameInputs[i])
                playerNameInputs[i].gameObject.SetActive(show);
            if (i < playerAvatarButtons.Length && playerAvatarButtons[i])
                playerAvatarButtons[i].gameObject.SetActive(show);
        }

        // Pre-fill names
        if (playerNameInputs.Length > 0 && playerNameInputs[0])
            playerNameInputs[0].text = GameSettings.LocalNickname;
        for (int i = 1; i < count; i++)
        {
            if (i < playerNameInputs.Length && playerNameInputs[i] != null &&
                string.IsNullOrEmpty(playerNameInputs[i].text))
                playerNameInputs[i].text = $"Игрок {i + 1}";
        }
    }

    void StartMultiplayer()
    {
        PlaySFX(cardShuffleSfx);
        var names = new string[selectedPlayerCount];
        for (int i = 0; i < selectedPlayerCount; i++)
        {
            bool hasInput = i < playerNameInputs.Length && playerNameInputs[i] != null;
            names[i] = hasInput && !string.IsNullOrWhiteSpace(playerNameInputs[i].text)
                ? playerNameInputs[i].text
                : $"Игрок {i + 1}";
        }
        // All human players (local coop from one PC)
        GameSettings.SetupLocalMultiplayer(names, null, selectedPlayerCount);
        SceneManager.LoadScene("GameScene");
    }

    void CloseMultiplayer()
    {
        PlaySFX(buttonSfx);
        if (multiplayerSetupPanel) multiplayerSetupPanel.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    void SetLang(string lang)
    {
        GameSettings.Language = lang;
        PlayerPrefs.SetString("Language", lang);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        GameSettings.MusicVolume   = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        GameSettings.SFXVolume     = PlayerPrefs.GetFloat("SFXVolume",   1.0f);
        GameSettings.LocalNickname = PlayerPrefs.GetString("Nickname",   "Игрок");
        GameSettings.Language      = PlayerPrefs.GetString("Language",   "RU");
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip)
            sfxSource.PlayOneShot(clip, GameSettings.SFXVolume);
    }
}
