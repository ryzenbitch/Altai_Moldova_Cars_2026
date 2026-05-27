// GonkiCards/Assets/Scripts/Editor/GonkiCardsBuilder.cs
// ─────────────────────────────────────────────────────────────────────────────
// ИНСТРУКЦИЯ:
//   1. Положи этот файл в Assets/Scripts/Editor/
//   2. В Unity меню появится  GonkiCards ▶ Build All Scenes
//   3. Нажми — и все три сцены соберутся автоматически за ~5 секунд
// ─────────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

public static class GonkiCardsBuilder
{
    // ── Colours ────────────────────────────────────────────────────────────
    static readonly Color COL_TABLE_BG = HEX("#3D5040");
    static readonly Color COL_PANEL_DARK = HEX("#1B2210");
    static readonly Color COL_BROWN = HEX("#5C3317");
    static readonly Color COL_GOLD = HEX("#FFD700");
    static readonly Color COL_TEXT_LIGHT = HEX("#F0EAD6");
    static readonly Color COL_BTN_GREEN = HEX("#2E7D32");
    static readonly Color COL_BTN_HOVER = HEX("#1B5E20");
    static readonly Color COL_CARD_BG = HEX("#2A1F3D");
    static readonly Color COL_STAT_DISP = HEX("#1A1A2E");
    static readonly Color COL_STAT_TEXT = HEX("#D4C87A");
    static readonly Color COL_RED_ELIM = HEX("#C0392B");

    // ── Entry points ───────────────────────────────────────────────────────
    [MenuItem("GonkiCards/Build All Scenes")]
    public static void BuildAll()
    {
        EnsureTMPImported();
        BuildMainMenu();
        BuildGameScene();
        BuildGalleryScene();
        SetBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("GonkiCards",
            "✅ Все три сцены собраны!\n\nОткрой MainMenu и нажми Play.", "OK");
    }

    [MenuItem("GonkiCards/Build MainMenu Only")]
    public static void BuildMainMenuOnly() { EnsureTMPImported(); BuildMainMenu(); AssetDatabase.SaveAssets(); }

    [MenuItem("GonkiCards/Build GameScene Only")]
    public static void BuildGameSceneOnly() { EnsureTMPImported(); BuildGameScene(); AssetDatabase.SaveAssets(); }

    [MenuItem("GonkiCards/Build GalleryScene Only")]
    public static void BuildGalleryOnly() { EnsureTMPImported(); BuildGalleryScene(); AssetDatabase.SaveAssets(); }

    // ══════════════════════════════════════════════════════════════════════
    //  MAIN MENU
    // ══════════════════════════════════════════════════════════════════════
    static void BuildMainMenu()
    {
        var scene = NewScene("Assets/Scenes/MainMenu.unity");

        // ── Camera ──
        var cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = COL_TABLE_BG;
        cam.tag = "MainCamera";

        // ── Bootstrapper ──
        var boot = new GameObject("[Bootstrapper]");
        boot.AddComponent<SceneBootstrapper>();

        // ── Canvas ──
        var canvas = MakeCanvas("Canvas");
        var canvasRect = canvas.GetComponent<RectTransform>();

        // Background
        var bg = MakeImage(canvas.transform, "Background", COL_PANEL_DARK);
        StretchFull(bg.GetComponent<RectTransform>());

        // Logo
        var logo = MakeTMP(canvas.transform, "Logo_Text", "GONKI\nCARDS",
            120, FontStyle.Bold, COL_GOLD);
        SetAnchored(logo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(500, 250));
        logo.alignment = TextAlignmentOptions.Center;

        var subtitle = MakeTMP(canvas.transform, "Subtitle",
            "Карточная игра про автомобили", 28, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(subtitle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(500, 40));
        subtitle.alignment = TextAlignmentOptions.Center;

        // Buttons container
        var btnContainer = MakeEmptyRect(canvas.transform, "ButtonsContainer");
        SetAnchored(btnContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -130), new Vector2(340, 360));
        var vl = btnContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 14; vl.childAlignment = TextAnchor.MiddleCenter;
        vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
        var csf = btnContainer.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        string[] btnLabels = { "🚗  Играть с ботом", "👥  Играть с другом",
                                "🖼  Галерея", "⚙  Настройки", "🚪  Выход" };
        string[] btnNames = { "btn_play_bot","btn_play_friend","btn_gallery",
                                "btn_settings","btn_exit" };
        var buttons = new List<Button>();
        foreach (var (lbl, name) in System.Linq.Enumerable.Zip(btnLabels, btnNames, (a, b) => (a, b)))
        {
            var btn = MakeMenuButton(btnContainer, name, lbl);
            buttons.Add(btn);
        }

        // ── Settings Panel ──
        var settingsPanel = MakePanel(canvas.transform, "SettingsPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 560));
        settingsPanel.SetActive(false);

        MakeTMP(settingsPanel.transform, "Title_Settings", "⚙ Настройки",
            36, FontStyle.Bold, COL_GOLD).GetComponent<RectTransform>()
            .anchoredPosition = new Vector2(0, 230);

        // Music slider
        MakeLabeledSlider(settingsPanel.transform, "MusicSlider", "🎵 Музыка", -80);
        MakeLabeledSlider(settingsPanel.transform, "SFXSlider", "🔊 Звуки", -150);

        var nickLabel = MakeTMP(settingsPanel.transform, "NickLabel", "Никнейм:",
            22, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(nickLabel.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-90, -5), new Vector2(160, 30));

        var nickInput = MakeInputField(settingsPanel.transform, "NicknameInput",
            "Vashe_Imya", new Vector2(80, -5), new Vector2(220, 40));

        var langLabel = MakeTMP(settingsPanel.transform, "LangLabel", "Язык / Language:",
            22, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(langLabel.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -65), new Vector2(300, 30));

        var langRU = MakeSmallButton(settingsPanel.transform, "LangRU_Button", "RU",
            new Vector2(-70, -105), new Vector2(100, 40));
        var langEN = MakeSmallButton(settingsPanel.transform, "LangEN_Button", "EN",
            new Vector2(70, -105), new Vector2(100, 40));
        var closeSettings = MakeSmallButton(settingsPanel.transform, "CloseSettingsButton",
            "✕ Закрыть", new Vector2(0, -195), new Vector2(200, 44));

        // ── Multiplayer Panel ──
        var multiPanel = MakePanel(canvas.transform, "MultiplayerSetupPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 620));
        multiPanel.SetActive(false);

        MakeTMP(multiPanel.transform, "Title_Multi", "👥 Игра с другом",
            36, FontStyle.Bold, COL_GOLD).GetComponent<RectTransform>()
            .anchoredPosition = new Vector2(0, 260);

        MakeTMP(multiPanel.transform, "HowMany", "Сколько игроков?",
            26, FontStyle.Normal, COL_TEXT_LIGHT).GetComponent<RectTransform>()
            .anchoredPosition = new Vector2(0, 205);

        var two = MakeSmallButton(multiPanel.transform, "TwoPlayersButton",
            "2 игрока", new Vector2(-80, 160), new Vector2(150, 44));
        var three = MakeSmallButton(multiPanel.transform, "ThreePlayersButton",
            "3 игрока", new Vector2(80, 160), new Vector2(150, 44));

        string[] pnames = { "Игрок 1", "Игрок 2", "Игрок 3" };
        var nameInputs = new TMP_InputField[3];
        var avatarBtns = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            float y = 95 - i * 65;
            nameInputs[i] = MakeInputField(multiPanel.transform,
                $"PlayerName_{i}", pnames[i], new Vector2(40, y), new Vector2(230, 44));
            avatarBtns[i] = MakeSmallButton(multiPanel.transform,
                $"AvatarBtn_{i}", "🖼", new Vector2(-100, y), new Vector2(44, 44));
        }

        var startMulti = MakeMenuButton(multiPanel.transform, "StartMultiButton", "🏁 Начать игру!");
        startMulti.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -110);
        startMulti.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 52);

        var closeMulti = MakeSmallButton(multiPanel.transform, "CloseMultiButton",
            "✕ Назад", new Vector2(0, -175), new Vector2(200, 44));

        // ── Audio sources ──
        var bgMusicGO = new GameObject("BGMusic"); bgMusicGO.transform.SetParent(canvas.transform);
        var bgMusic = bgMusicGO.AddComponent<AudioSource>();
        bgMusic.loop = true; bgMusic.playOnAwake = false;
        var sfxGO = new GameObject("SFX"); sfxGO.transform.SetParent(canvas.transform);
        sfxGO.AddComponent<AudioSource>();

        // ── Wire MainMenuController ──
        var ctrlGO = new GameObject("[MainMenuController]");
        var ctrl = ctrlGO.AddComponent<MainMenuController>();

        ctrl.playWithBotButton = buttons[0];
        ctrl.playWithFriendButton = buttons[1];
        ctrl.galleryButton = buttons[2];
        ctrl.settingsButton = buttons[3];
        ctrl.exitButton = buttons[4];

        ctrl.settingsPanel = settingsPanel;
        ctrl.musicSlider = settingsPanel.transform.Find("MusicSlider/Slider")?.GetComponent<Slider>();
        ctrl.sfxSlider = settingsPanel.transform.Find("SFXSlider/Slider")?.GetComponent<Slider>();
        ctrl.nicknameInput = nickInput;
        ctrl.langRuButton = langRU;
        ctrl.langEnButton = langEN;
        ctrl.closeSettingsButton = closeSettings;

        ctrl.multiplayerSetupPanel = multiPanel;
        ctrl.twoPlayersButton = two;
        ctrl.threePlayersButton = three;
        ctrl.playerNameInputs = nameInputs;
        ctrl.playerAvatarButtons = avatarBtns;
        ctrl.startMultiButton = startMulti;
        ctrl.closeMultiButton = closeMulti;
        ctrl.bgMusic = bgMusic;
        // sfxSource auto-created // ctrl.sfxSource = sfxGO.GetComponent<AudioSource>();

        SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("✅ MainMenu built");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GAME SCENE
    // ══════════════════════════════════════════════════════════════════════
    static void BuildGameScene()
    {
        var scene = NewScene("Assets/Scenes/GameScene.unity");

        var cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = COL_TABLE_BG;
        cam.tag = "MainCamera";

        // Managers
        var gmGO = new GameObject("[GameManager]"); gmGO.AddComponent<GameManager>();
        var boot = new GameObject("[Bootstrapper]"); boot.AddComponent<SceneBootstrapper>();
        var dealGO = new GameObject("[CardDealAnimator]"); dealGO.AddComponent<CardDealAnimator>();

        // Canvas
        var canvas = MakeCanvas("Canvas");

        // ── BG ──
        var bg = MakeImage(canvas.transform, "TableBackground", COL_TABLE_BG);
        StretchFull(bg.GetComponent<RectTransform>());

        // ── HUD Top Bar ──
        // Height = 80px, anchored to top edge, spans full width
        var hudBar = MakeImage(canvas.transform, "HUDBar", COL_PANEL_DARK);
        var hudRT = hudBar.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 1);
        hudRT.anchorMax = new Vector2(1, 1);
        hudRT.offsetMin = new Vector2(0, -80);
        hudRT.offsetMax = new Vector2(0, 0);

        // Round counter centred in HUD bar
        var roundTxt = MakeTMP(canvas.transform, "RoundCounterText", "Раунд 1",
            26, FontStyle.Bold, COL_GOLD);
        SetAnchored(roundTxt.GetComponent<RectTransform>(),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(300, 40));
        roundTxt.alignment = TextAlignmentOptions.Center;

        // Turn banner — left side of HUD bar
        var turnBanner = MakeTMP(canvas.transform, "TurnBannerText", "Ходит: ...",
            22, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(turnBanner.GetComponent<RectTransform>(),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -40), new Vector2(400, 40));

        // MY TURN indicator
        var myTurnBanner = MakePanel(canvas.transform, "MyTurnBanner",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(400, 70));
        myTurnBanner.GetComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.92f);
        MakeTMP(myTurnBanner.transform, "MyTurnText", "🚗  ВАШ ХОД!",
            38, FontStyle.Bold, COL_PANEL_DARK).alignment = TextAlignmentOptions.Center;
        SetAnchored(myTurnBanner.transform.GetChild(0).GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetAnchored(myTurnBanner.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(400, 70));
        myTurnBanner.SetActive(false);

        // ── OPPONENT SLOTS (top HUD bar: left quarter and right quarter) ──
        // На скрине: слот 1 слева (~25% по X), слот 2 справа (~75% по X), оба в HUD-баре
        var oppSlot1GO = BuildPlayerSlot(canvas.transform, "OpponentSlot_1",
            new Vector2(0.25f, 1), new Vector2(-240, -40), false);
        var oppSlot2GO = BuildPlayerSlot(canvas.transform, "OpponentSlot_2",
            new Vector2(0.75f, 1), new Vector2(240, -40), false);

        // Opponent card displays — два пустых места с пунктирной рамкой (центр экрана, правее карты игрока)
        // На скрине: по центру-правее, вертикальная середина
        var oppCard1 = BuildCardDisplay(canvas.transform, "OppCard_1",
            new Vector2(0.5f, 0.5f), new Vector2(-160, 30), 0.55f);
        var oppCard2 = BuildCardDisplay(canvas.transform, "OppCard_2",
            new Vector2(0.75f, 0.5f), new Vector2(0, 30), 0.55f);

        // Пунктирные рамки для пустых слотов карт (визуальный placeholder)
        foreach (var oppCard in new[] { oppCard1, oppCard2 })
        {
            var placeholderBorder = MakeImage(oppCard.transform, "DashedBorder",
                new Color(1f, 1f, 1f, 0.4f));
            StretchFull(placeholderBorder.GetComponent<RectTransform>());
            var outline = placeholderBorder.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.7f);
            outline.effectDistance = new Vector2(3, -3);
            placeholderBorder.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // прозрачный fill
        }

        // ── MY SLOT (bottom right) ──
        // На скрине: ник "Vashe_Imya" с аватаркой внизу справа
        var mySlotGO = BuildPlayerSlot(canvas.transform, "MySlot",
            new Vector2(1f, 0), new Vector2(-200, 60), true);

        // My card display — слева по центру вертикально, крупно
        var myCard = BuildCardDisplay(canvas.transform, "MyCard",
            new Vector2(0.14f, 0.5f), new Vector2(0, 20), 0.72f);

        // Deck counters (in player slots)
        var myCounter = mySlotGO.transform.Find("DeckCounter")?.GetComponent<DeckCounterUI>();

        // ── DECK / RESERVE COUNTERS (bottom-left circles, как на скрине) ──
        // Левый круг: "10 / В колоде"
        var deckCircle = MakeImage(canvas.transform, "DeckCirclePanel", COL_BROWN);
        SetAnchored(deckCircle.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(130, 130), new Vector2(200, 200));
        var deckCircleImg = deckCircle.GetComponent<Image>();
        // Сделаем круг через sprite type — если нет спрайта, просто квадрат с закруглением (в рантайме через outline)

        var deckNumTxt = MakeTMP(deckCircle.transform, "DeckCountBig", "10",
            64, FontStyle.Bold, Color.white);
        SetAnchored(deckNumTxt.GetComponent<RectTransform>(),
            new Vector2(0, 0.45f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        deckNumTxt.alignment = TextAlignmentOptions.Center;

        var deckLabelTxt = MakeTMP(deckCircle.transform, "DeckLabel", "В колоде",
            18, FontStyle.Bold, Color.white);
        SetAnchored(deckLabelTxt.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(1, 0.45f), Vector2.zero, Vector2.zero);
        deckLabelTxt.alignment = TextAlignmentOptions.Center;

        // Правый круг: "7 / В резерве"
        var resCircle = MakeImage(canvas.transform, "ReserveCirclePanel", new Color(0.35f, 0.18f, 0.08f));
        SetAnchored(resCircle.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(310, 130), new Vector2(200, 200));

        var resNumTxt = MakeTMP(resCircle.transform, "ReserveCountBig", "7",
            64, FontStyle.Bold, Color.white);
        SetAnchored(resNumTxt.GetComponent<RectTransform>(),
            new Vector2(0, 0.45f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        resNumTxt.alignment = TextAlignmentOptions.Center;

        var resLabelTxt = MakeTMP(resCircle.transform, "ReserveLabel", "В резерве",
            18, FontStyle.Bold, Color.white);
        SetAnchored(resLabelTxt.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(1, 0.45f), Vector2.zero, Vector2.zero);
        resLabelTxt.alignment = TextAlignmentOptions.Center;

        // ── STAT PANEL ──
        var statPanel = MakePanel(canvas.transform, "StatPanel",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(290, 480));
        statPanel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 0.97f);

        MakeTMP(statPanel.transform, "StatPanelTitle", "Выберите\nхарактеристику:",
            22, FontStyle.Bold, COL_GOLD).GetComponent<RectTransform>().anchoredPosition
            = new Vector2(0, 195);

        string[] statLabels = {"⏱ 0-100 (сек)","⚡ Мощность","💨 Макс. скорость",
                               "🔧 Объём двигателя","⚖ Масса"};
        string[] statHints = {"↓ меньше лучше","↑ больше лучше","↑ больше лучше",
                               "↑ больше лучше","↓ меньше лучше"};
        var statBtns = new StatButtonUI[5];
        for (int i = 0; i < 5; i++)
        {
            float y = 135 - i * 76;
            var sb = MakeStatButton(statPanel.transform, $"StatBtn_{i}",
                statLabels[i], statHints[i], new Vector2(0, y));
            statBtns[i] = sb;
        }
        statPanel.SetActive(false);

        // ── ROUND RESULT PANEL ──
        var rrPanel = MakePanel(canvas.transform, "RoundResultPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 460));
        rrPanel.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.12f, 0.97f);

        var rrComp = rrPanel.AddComponent<RoundResultPanel>();

        var rrHeadline = MakeTMP(rrPanel.transform, "RoundResultHeadline", "",
            36, FontStyle.Bold, COL_GOLD);
        SetAnchored(rrHeadline.GetComponent<RectTransform>(),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -45), new Vector2(700, 60));
        rrHeadline.alignment = TextAlignmentOptions.Center;

        var rrStatText = MakeTMP(rrPanel.transform, "SelectedStatText", "",
            24, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(rrStatText.GetComponent<RectTransform>(),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(600, 36));
        rrStatText.alignment = TextAlignmentOptions.Center;

        // 3 mini card displays inside result panel
        float[] xPos = { -290f, 0f, 290f };
        var rrCardDisplays = new CardDisplayUI[3];
        var rrPlayerNames = new TextMeshProUGUI[3];
        var rrCrowns = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var miniCard = BuildCardDisplay(rrPanel.transform, $"ResultCard_{i}",
                new Vector2(0.5f, 0.5f), new Vector2(xPos[i], -30), 0.42f);
            rrCardDisplays[i] = miniCard.GetComponent<CardDisplayUI>();

            var pname = MakeTMP(rrPanel.transform, $"RRPlayerName_{i}", "",
                20, FontStyle.Bold, COL_TEXT_LIGHT);
            SetAnchored(pname.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(xPos[i], 60), new Vector2(240, 30));
            pname.alignment = TextAlignmentOptions.Center;
            rrPlayerNames[i] = pname;

            var crown = MakeTMP(rrPanel.transform, $"Crown_{i}", "👑",
                40, FontStyle.Normal, COL_GOLD);
            SetAnchored(crown.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(xPos[i], 90), new Vector2(60, 55));
            crown.alignment = TextAlignmentOptions.Center;
            rrCrowns[i] = crown.gameObject;
            rrCrowns[i].SetActive(false);
        }
        var rrContinue = MakeSmallButton(rrPanel.transform, "ContinueButton",
            "Продолжить →", new Vector2(0, -195), new Vector2(200, 44));

        rrComp.resultCardDisplays = rrCardDisplays;
        rrComp.resultPlayerNames = rrPlayerNames;
        rrComp.winnerCrowns = rrCrowns;
        rrComp.roundResultHeadline = rrHeadline;
        rrComp.selectedStatText = rrStatText;
        rrComp.continueButton = rrContinue;
        rrPanel.SetActive(false);

        // ── ELIMINATION NOTICE ──
        var elimNotice = MakePanel(canvas.transform, "EliminationNotice",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(520, 90));
        elimNotice.GetComponent<Image>().color = new Color(COL_RED_ELIM.r, COL_RED_ELIM.g, COL_RED_ELIM.b, 0.93f);
        var elimText = MakeTMP(elimNotice.transform, "EliminationText", "Игрок выбывает!",
            30, FontStyle.Bold, Color.white);
        SetAnchored(elimText.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        elimText.alignment = TextAlignmentOptions.Center;
        elimNotice.SetActive(false);

        // ── GAME OVER SCREEN ──
        var goScreen = MakePanel(canvas.transform, "GameOverScreen",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        StretchFull(goScreen.GetComponent<RectTransform>());
        goScreen.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 0.97f);
        var goComp = goScreen.AddComponent<GameOverScreen>();
        var goCG = goScreen.AddComponent<CanvasGroup>();
        goComp.canvasGroup = goCG;

        var goHeadline = MakeTMP(goScreen.transform, "GOHeadline", "🏆 ВЫ ПОБЕДИЛИ!",
            64, FontStyle.Bold, COL_GOLD);
        SetAnchored(goHeadline.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(700, 90));
        goHeadline.alignment = TextAlignmentOptions.Center;

        var goWinnerName = MakeTMP(goScreen.transform, "GOWinnerName", "",
            36, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(goWinnerName.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(500, 50));
        goWinnerName.alignment = TextAlignmentOptions.Center;

        var goStats = MakeTMP(goScreen.transform, "GOStats", "",
            24, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(goStats.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(400, 40));
        goStats.alignment = TextAlignmentOptions.Center;

        var goPlayAgain = MakeMenuButton(goScreen.transform, "PlayAgainBtn", "🔄 Играть снова");
        SetAnchored(goPlayAgain.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-170, -80), new Vector2(290, 56));

        var goMainMenu = MakeMenuButton(goScreen.transform, "MainMenuBtn", "🏠 Главное меню");
        SetAnchored(goMainMenu.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(170, -80), new Vector2(290, 56));

        goComp.headlineText = goHeadline;
        goComp.winnerNameText = goWinnerName;
        goComp.statsText = goStats;
        goComp.playAgainButton = goPlayAgain;
        goComp.mainMenuButton = goMainMenu;
        goScreen.SetActive(false);

        // ── GameSceneController ──
        var ctrlGO = new GameObject("[GameSceneController]");
        var ctrl = ctrlGO.AddComponent<GameSceneController>();

        ctrl.playerSlots = new PlayerSlotUI[] {
            oppSlot1GO.GetComponent<PlayerSlotUI>(),
            oppSlot2GO.GetComponent<PlayerSlotUI>(),
            mySlotGO.GetComponent<PlayerSlotUI>()
        };
        ctrl.deckCounters = new DeckCounterUI[] {
            oppSlot1GO.GetComponentInChildren<DeckCounterUI>(),
            oppSlot2GO.GetComponentInChildren<DeckCounterUI>(),
            mySlotGO.GetComponentInChildren<DeckCounterUI>()
        };
        ctrl.opponentCardDisplays = new CardDisplayUI[] {
            oppCard1.GetComponent<CardDisplayUI>(),
            oppCard2.GetComponent<CardDisplayUI>()
        };
        ctrl.myCardDisplay = myCard.GetComponent<CardDisplayUI>();
        ctrl.statPanel = statPanel;
        ctrl.statButtons = statBtns;
        ctrl.roundResultPanel = rrComp;
        ctrl.turnBannerText = turnBanner;
        ctrl.myTurnBanner = myTurnBanner;
        ctrl.roundCounterText = roundTxt;
        ctrl.eliminationNotice = elimNotice;
        ctrl.eliminationText = elimText;
        ctrl.gameOverScreen = goComp;

        SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("✅ GameScene built");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GALLERY SCENE
    // ══════════════════════════════════════════════════════════════════════
    static void BuildGalleryScene()
    {
        var scene = NewScene("Assets/Scenes/GalleryScene.unity");

        var cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = COL_PANEL_DARK;
        cam.tag = "MainCamera";

        var boot = new GameObject("[Bootstrapper]"); boot.AddComponent<SceneBootstrapper>();

        var canvas = MakeCanvas("Canvas");

        var bg = MakeImage(canvas.transform, "Background", COL_PANEL_DARK);
        StretchFull(bg.GetComponent<RectTransform>());

        // Title
        var title = MakeTMP(canvas.transform, "GalleryTitle", "🏎  Галерея автомобилей",
            42, FontStyle.Bold, COL_GOLD);
        SetAnchored(title.GetComponent<RectTransform>(),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -55), new Vector2(700, 70));
        title.alignment = TextAlignmentOptions.Center;

        // Back button
        var backBtn = MakeSmallButton(canvas.transform, "BackButton",
            "← Назад", new Vector2(-860, 0), new Vector2(150, 44));
        SetAnchored(backBtn.GetComponent<RectTransform>(),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -45), new Vector2(150, 44));

        // ScrollView
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(canvas.transform, false);
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        var scrollRT = scrollGO.AddComponent<RectTransform>() != null
            ? scrollGO.GetComponent<RectTransform>() : scrollGO.GetComponent<RectTransform>();
        SetAnchored(scrollRT, new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(0, -110), new Vector2(0, 0));
        scrollRT.offsetMin = new Vector2(20, 20);
        scrollRT.offsetMax = new Vector2(-20, -110);

        scrollGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);

        var viewport = MakeEmptyRect(scrollGO.transform, "Viewport");
        StretchFull(viewport);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);

        var content = MakeEmptyRect(viewport, "Content");
        SetAnchored(content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 0));
        content.anchoredPosition = Vector2.zero;
        var grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(190, 268);
        grid.spacing = new Vector2(18, 18);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.Flexible;
        var csf2 = content.gameObject.AddComponent<ContentSizeFitter>();
        csf2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30;

        // Build CardThumbnail prefab on the fly (stored as child template, then cleared)
        var thumbTemplate = BuildCardThumbnailPrefab(content);
        // Save as prefab
        string prefabPath = "Assets/Prefabs/CardThumbnail.prefab";
        Directory.CreateDirectory("Assets/Prefabs");
        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(thumbTemplate, prefabPath);
        Object.DestroyImmediate(thumbTemplate);

        // Detail Panel
        var detailPanel = MakePanel(canvas.transform, "DetailPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780, 560));
        detailPanel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.18f, 0.98f);
        detailPanel.SetActive(false);

        var detailCarImg = MakeImage(detailPanel.transform, "DetailCarImage", Color.white);
        SetAnchored(detailCarImg.GetComponent<RectTransform>(),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(180, 0), new Vector2(300, 420));
        detailCarImg.preserveAspect = true;

        var detailName = MakeTMP(detailPanel.transform, "DetailCarName", "",
            30, FontStyle.Bold, COL_GOLD);
        SetAnchored(detailName.GetComponent<RectTransform>(),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-185, -50), new Vector2(330, 46));

        var detailModel = MakeTMP(detailPanel.transform, "DetailCarModel", "",
            20, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(detailModel.GetComponent<RectTransform>(),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-185, -95), new Vector2(330, 30));

        string[] dStatNames = {"DetailAcceleration","DetailHorsepower","DetailMaxSpeed",
                               "DetailEngineVolume","DetailWeight"};
        var dStatFields = new TextMeshProUGUI[5];
        for (int i = 0; i < 5; i++)
        {
            dStatFields[i] = MakeTMP(detailPanel.transform, dStatNames[i], "",
                22, FontStyle.Normal, COL_STAT_TEXT);
            SetAnchored(dStatFields[i].GetComponent<RectTransform>(),
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-185, 70 - i * 42), new Vector2(330, 34));
        }

        var detailDesc = MakeTMP(detailPanel.transform, "DetailDescription", "",
            18, FontStyle.Italic, COL_TEXT_LIGHT);
        SetAnchored(detailDesc.GetComponent<RectTransform>(),
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-185, 90), new Vector2(330, 70));
        detailDesc.enableWordWrapping = true;

        var detailPrice = MakeTMP(detailPanel.transform, "DetailPrice", "",
            26, FontStyle.Bold, COL_GOLD);
        SetAnchored(detailPrice.GetComponent<RectTransform>(),
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-185, 52), new Vector2(330, 38));

        var detailRarity = MakeTMP(detailPanel.transform, "DetailRarity", "",
            20, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(detailRarity.GetComponent<RectTransform>(),
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-185, 20), new Vector2(330, 30));

        var prevBtn = MakeSmallButton(detailPanel.transform, "PrevButton",
            "◀ Пред.", new Vector2(-250, -240), new Vector2(150, 44));
        var nextBtn = MakeSmallButton(detailPanel.transform, "NextButton",
            "След. ▶", new Vector2(100, -240), new Vector2(150, 44));
        var closeDetail = MakeSmallButton(detailPanel.transform, "CloseDetailButton",
            "✕ Закрыть", new Vector2(-60, -240), new Vector2(110, 44));

        // Wire GalleryController
        var ctrlGO = new GameObject("[GalleryController]");
        var ctrl = ctrlGO.AddComponent<GalleryController>();
        ctrl.cardGridParent = content;
        ctrl.cardThumbnailPrefab = savedPrefab;
        ctrl.scrollRect = scrollRect;
        ctrl.detailPanel = detailPanel;
        ctrl.detailCarImage = detailCarImg;
        ctrl.detailCarName = detailName;
        ctrl.detailCarModel = detailModel;
        ctrl.detailDescription = detailDesc;
        ctrl.detailRarity = detailRarity;
        ctrl.detailPrice = detailPrice;
        ctrl.detailAcceleration = dStatFields[0];
        ctrl.detailHorsepower = dStatFields[1];
        ctrl.detailMaxSpeed = dStatFields[2];
        ctrl.detailEngineVolume = dStatFields[3];
        ctrl.detailWeight = dStatFields[4];
        ctrl.closeDetailButton = closeDetail;
        ctrl.prevCardButton = prevBtn;
        ctrl.nextCardButton = nextBtn;
        ctrl.backButton = backBtn;

        SaveScene(scene, "Assets/Scenes/GalleryScene.unity");
        Debug.Log("✅ GalleryScene built");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPER: Build Player Slot
    // ══════════════════════════════════════════════════════════════════════
    static GameObject BuildPlayerSlot(Transform parent, string name,
        Vector2 anchorPos, Vector2 offset, bool isLocalPlayer)
    {
        var slotGO = MakePanel(parent, name,
            anchorPos, anchorPos, offset,
            new Vector2(isLocalPlayer ? 340 : 380, isLocalPlayer ? 90 : 80));
        slotGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // прозрачный фон — HUD сам тёмный

        // Active turn glow
        var glow = MakeImage(slotGO.transform, "ActiveGlow", new Color(1f, 0.84f, 0f, 0.25f));
        StretchFull(glow.GetComponent<RectTransform>());
        glow.gameObject.SetActive(false);

        // Avatar circle
        var avatar = MakeImage(slotGO.transform, "AvatarImage", COL_BROWN);
        float av = isLocalPlayer ? 70 : 60;
        SetAnchored(avatar.GetComponent<RectTransform>(),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(av * 0.5f + 8, 0), new Vector2(av, av));

        // Nickname
        float txtX = isLocalPlayer ? 110 : 94;
        var nick = MakeTMP(slotGO.transform, "NicknameText", "Игрок",
            isLocalPlayer ? 22 : 20, FontStyle.Bold, COL_GOLD);
        SetAnchored(nick.GetComponent<RectTransform>(),
            new Vector2(0, 0.5f), new Vector2(0.6f, 0.5f),
            new Vector2(txtX, 8), new Vector2(-(txtX + 6), 28));

        // Score circles (видны на скрине как круглые жёлтые/белые кружки с цифрами рядом с ником)
        // Для противников: два кружка с цифрами (раунды выиграны / проиграны)
        if (!isLocalPlayer)
        {
            var scoreBox1 = MakeImage(slotGO.transform, "ScoreCircle1", new Color(0.85f, 0.65f, 0.1f));
            SetAnchored(scoreBox1.GetComponent<RectTransform>(),
                new Vector2(0.62f, 0.5f), new Vector2(0.62f, 0.5f),
                new Vector2(22, 0), new Vector2(36, 36));
            var score1Txt = MakeTMP(scoreBox1.transform, "Score1", "0",
                18, FontStyle.Bold, Color.white);
            StretchFull(score1Txt.GetComponent<RectTransform>());
            score1Txt.alignment = TextAlignmentOptions.Center;

            var scoreBox2 = MakeImage(slotGO.transform, "ScoreCircle2", new Color(0.5f, 0.5f, 0.5f));
            SetAnchored(scoreBox2.GetComponent<RectTransform>(),
                new Vector2(0.62f, 0.5f), new Vector2(0.62f, 0.5f),
                new Vector2(66, 0), new Vector2(36, 36));
            var score2Txt = MakeTMP(scoreBox2.transform, "Score2", "0",
                18, FontStyle.Bold, Color.white);
            StretchFull(score2Txt.GetComponent<RectTransform>());
            score2Txt.alignment = TextAlignmentOptions.Center;
        }

        // Deck counter sub-object
        var counterGO = new GameObject("DeckCounter"); counterGO.transform.SetParent(slotGO.transform, false);
        var counter = counterGO.AddComponent<DeckCounterUI>();
        var dcRT = counterGO.AddComponent<RectTransform>() != null
            ? counterGO.GetComponent<RectTransform>() : counterGO.GetComponent<RectTransform>();
        SetAnchored(dcRT, new Vector2(0, 0.5f), new Vector2(1, 0.5f),
            new Vector2(txtX, -14), new Vector2(-(txtX + 6), 22));

        var deckTxt = MakeTMP(counterGO.transform, "DeckCount", "Колода: 6",
            14, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(deckTxt.GetComponent<RectTransform>(),
            new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        StretchFull(deckTxt.GetComponent<RectTransform>());

        var resTxt = MakeTMP(counterGO.transform, "ReserveCount", "Резерв: 0",
            14, FontStyle.Normal, new Color(0.7f, 0.9f, 0.7f));
        SetAnchored(resTxt.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(1, 0.5f), Vector2.zero, Vector2.zero);
        StretchFull(resTxt.GetComponent<RectTransform>());

        counter.deckCountText = deckTxt;
        counter.reserveCountText = resTxt;

        // Eliminated overlay
        var elimOv = MakeImage(slotGO.transform, "EliminatedOverlay",
            new Color(0.6f, 0.05f, 0.05f, 0.78f));
        StretchFull(elimOv.GetComponent<RectTransform>());
        MakeTMP(elimOv.transform, "ElimText", "ВЫБЫЛ",
            22, FontStyle.Bold, Color.white).alignment = TextAlignmentOptions.Center;
        elimOv.gameObject.SetActive(false);

        var slotComp = slotGO.AddComponent<PlayerSlotUI>();
        slotComp.avatarImage = avatar.GetComponent<Image>();
        slotComp.nicknameText = nick;
        slotComp.activeTurnGlow = glow.gameObject;
        slotComp.eliminatedOverlay = elimOv.gameObject;
        slotComp.borderImage = slotGO.GetComponent<Image>();

        return slotGO;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPER: Build Card Display
    // ══════════════════════════════════════════════════════════════════════
    static GameObject BuildCardDisplay(Transform parent, string name,
        Vector2 anchor, Vector2 offset, float scale)
    {
        float w = 547 * scale, h = 768 * scale;
        var cardRoot = MakeEmptyRect(parent, name);
        SetAnchored(cardRoot, anchor, anchor, offset, new Vector2(w, h));

        // Back
        var back = MakeImage(cardRoot, "CardBack", new Color(0.12f, 0.08f, 0.22f));
        StretchFull(back.GetComponent<RectTransform>());
        MakeTMP(back.transform, "BackText", "?",
            (int)(80 * scale), FontStyle.Bold, new Color(0.4f, 0.2f, 0.7f)).alignment
            = TextAlignmentOptions.Center;
        var backTextRT = back.transform.GetChild(0).GetComponent<RectTransform>();
        StretchFull(backTextRT);

        // Front
        var front = MakeEmptyRect(cardRoot, "CardFront");
        StretchFull(front);
        front.gameObject.SetActive(false);

        // BG gradient
        var frontBG = MakeImage(front, "CardBackground", COL_CARD_BG);
        StretchFull(frontBG.GetComponent<RectTransform>());

        // Car Image (top 55%)
        var carImg = MakeImage(front, "CarImage", Color.white);
        carImg.preserveAspect = true;
        SetAnchored(carImg.GetComponent<RectTransform>(),
            new Vector2(0, 0.45f), new Vector2(1, 1), new Vector2(4, -4), new Vector2(-4, 0));

        // ID badge
        int fs = Mathf.Max(12, (int)(22 * scale));
        var idBadge = MakeTMP(front, "CardIdText", "?", fs, FontStyle.Bold, Color.white);
        SetAnchored(idBadge.GetComponent<RectTransform>(),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(8 + fs, -8 - fs), new Vector2(fs * 2, fs * 2));
        idBadge.alignment = TextAlignmentOptions.Center;

        // Rarity
        var rarityT = MakeTMP(front, "RarityText", "О", fs, FontStyle.Bold, COL_GOLD);
        SetAnchored(rarityT.GetComponent<RectTransform>(),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-8 - fs, -8 - fs), new Vector2(fs * 2, fs * 2));
        rarityT.alignment = TextAlignmentOptions.Center;

        // Car name
        int nameFS = Mathf.Max(10, (int)(18 * scale));
        var carName = MakeTMP(front, "CarNameText", "", nameFS, FontStyle.Bold, Color.white);
        SetAnchored(carName.GetComponent<RectTransform>(),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(8 + nameFS * 2, -8), new Vector2(-(nameFS * 4 + 16), nameFS + 6));
        carName.alignment = TextAlignmentOptions.Center;

        // Model
        int modelFS = Mathf.Max(8, (int)(13 * scale));
        var carModel = MakeTMP(front, "CarModelText", "", modelFS, FontStyle.Normal,
            new Color(0.8f, 0.8f, 0.8f));
        SetAnchored(carModel.GetComponent<RectTransform>(),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -8 - nameFS - 2), new Vector2(-16, modelFS + 4));
        carModel.alignment = TextAlignmentOptions.Center;

        // Stats area (bottom 45%)
        float statW = w * 0.9f; float statH = h * 0.38f;
        var statsArea = MakeEmptyRect(front, "StatsArea");
        SetAnchored(statsArea, new Vector2(0.05f, 0), new Vector2(0.95f, 0.44f),
            new Vector2(0, 4), new Vector2(0, -4));

        // 5 stat displays
        string[] sNames = {"AccelerationText","HorsepowerText","MaxSpeedText",
                           "EngineVolumeText","WeightText"};
        string[] sLabels = {"0-100\nsec","Мощность\nл.с.","Макс.\nкм/ч",
                           "Объём\nсм³","Масса\nкг"};
        int sFS = Mathf.Max(9, (int)(14 * scale));
        var statTexts = new TextMeshProUGUI[5];
        // top row: 3 stats
        for (int i = 0; i < 3; i++)
        {
            float xn = (i / (2f)) * 1f; // 0, 0.5, 1
            var statBox = MakeImage(statsArea, $"StatBox_{i}", COL_STAT_DISP);
            SetAnchored(statBox.GetComponent<RectTransform>(),
                new Vector2(i * 0.34f, 0.52f), new Vector2(i * 0.34f + 0.3f, 1f),
                new Vector2(3, 3), new Vector2(-3, -3));
            var lbl = MakeTMP(statBox.transform, $"Lbl{i}", sLabels[i],
                Mathf.Max(7, (int)(9 * scale)), FontStyle.Normal, new Color(0.75f, 0.75f, 0.75f));
            StretchFull(lbl.GetComponent<RectTransform>()); lbl.alignment = TextAlignmentOptions.Center;
            var val = MakeTMP(statBox.transform, sNames[i], "—", sFS, FontStyle.Bold, COL_STAT_TEXT);
            SetAnchored(val.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(1, 0.55f), Vector2.zero, Vector2.zero);
            val.alignment = TextAlignmentOptions.Center;
            statTexts[i] = val;
        }
        // bottom row: 2 stats
        for (int i = 0; i < 2; i++)
        {
            var statBox = MakeImage(statsArea, $"StatBox_B{i}", COL_STAT_DISP);
            SetAnchored(statBox.GetComponent<RectTransform>(),
                new Vector2(i * 0.52f, 0), new Vector2(i * 0.52f + 0.46f, 0.48f),
                new Vector2(3, 3), new Vector2(-3, -3));
            var lbl = MakeTMP(statBox.transform, $"LblB{i}", sLabels[3 + i],
                Mathf.Max(7, (int)(9 * scale)), FontStyle.Normal, new Color(0.75f, 0.75f, 0.75f));
            StretchFull(lbl.GetComponent<RectTransform>()); lbl.alignment = TextAlignmentOptions.Center;
            var val = MakeTMP(statBox.transform, sNames[3 + i], "—", sFS, FontStyle.Bold, COL_STAT_TEXT);
            SetAnchored(val.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(1, 0.55f), Vector2.zero, Vector2.zero);
            val.alignment = TextAlignmentOptions.Center;
            statTexts[3 + i] = val;
        }

        // Price
        int priceFS = Mathf.Max(10, (int)(16 * scale));
        var priceT = MakeTMP(front, "PriceText", "$ —", priceFS, FontStyle.Bold, COL_GOLD);
        SetAnchored(priceT.GetComponent<RectTransform>(),
            new Vector2(0, 0.44f), new Vector2(1, 0.44f), new Vector2(0, -priceFS - 2), new Vector2(0, priceFS + 4));
        priceT.alignment = TextAlignmentOptions.Center;

        // Description (very small, bottom)
        int descFS = Mathf.Max(7, (int)(10 * scale));
        var descT = MakeTMP(front, "DescriptionText", "", descFS, FontStyle.Italic,
            new Color(0.75f, 0.75f, 0.75f));
        SetAnchored(descT.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(4, 4), new Vector2(-4, h * 0.08f));
        descT.enableWordWrapping = true;
        descT.alignment = TextAlignmentOptions.Center;

        // Stat highlight overlays
        var highlights = new Image[5];
        for (int i = 0; i < 3; i++)
        {
            var h2 = MakeImage(front, $"Highlight_{i}", Color.clear);
            var sbox = statsArea.Find($"StatBox_{i}");
            if (sbox)
            {
                var sboxRT = sbox.GetComponent<RectTransform>();
                h2.GetComponent<RectTransform>().SetParent(sbox, false);
                StretchFull(h2.GetComponent<RectTransform>());
            }
            highlights[i] = h2.GetComponent<Image>();
        }
        for (int i = 0; i < 2; i++)
        {
            var h2 = MakeImage(front, $"Highlight_B{i}", Color.clear);
            var sbox = statsArea.Find($"StatBox_B{i}");
            if (sbox)
            {
                h2.GetComponent<RectTransform>().SetParent(sbox, false);
                StretchFull(h2.GetComponent<RectTransform>());
            }
            highlights[3 + i] = h2.GetComponent<Image>();
        }

        // Wire CardDisplayUI
        var cdUI = cardRoot.gameObject.AddComponent<CardDisplayUI>();
        cdUI.cardFront = front.gameObject;
        cdUI.cardBack = back.gameObject;
        cdUI.carImage = carImg.GetComponent<Image>();
        cdUI.cardIdText = idBadge;
        cdUI.rarityText = rarityT;
        cdUI.carNameText = carName;
        cdUI.carModelText = carModel;
        cdUI.accelerationText = statTexts[0];
        cdUI.horsepowerText = statTexts[1];
        cdUI.maxSpeedText = statTexts[2];
        cdUI.engineVolumeText = statTexts[3];
        cdUI.weightText = statTexts[4];
        cdUI.priceText = priceT;
        cdUI.descriptionText = descT;
        cdUI.statHighlights = highlights;

        return cardRoot.gameObject;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPER: Stat Button
    // ══════════════════════════════════════════════════════════════════════
    static StatButtonUI MakeStatButton(Transform parent, string name,
        string label, string hint, Vector2 pos)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchored(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(264, 64));
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 0.95f);
        go.AddComponent<Button>();

        var statName = MakeTMP(go.transform, "StatName", label,
            18, FontStyle.Bold, COL_TEXT_LIGHT);
        SetAnchored(statName.GetComponent<RectTransform>(),
            new Vector2(0, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(8, 8), new Vector2(-8, -8));

        var statVal = MakeTMP(go.transform, "StatValue", "—",
            16, FontStyle.Normal, COL_STAT_TEXT);
        SetAnchored(statVal.GetComponent<RectTransform>(),
            new Vector2(0.7f, 0.5f), new Vector2(1, 0.5f), new Vector2(2, 0), new Vector2(-4, 0));
        statVal.alignment = TextAlignmentOptions.Right;

        // hint is encoded in label for StatButtonUI.Setup()
        var comp = go.AddComponent<StatButtonUI>();
        comp.statNameText = statName;
        comp.statValueText = statVal;
        comp.background = go.GetComponent<Image>();
        return comp;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPER: Card Thumbnail Prefab
    // ══════════════════════════════════════════════════════════════════════
    static GameObject BuildCardThumbnailPrefab(Transform parent)
    {
        var go = new GameObject("CardThumbnail"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(190, 268);

        var bg = go.AddComponent<Image>(); bg.color = COL_CARD_BG;
        go.AddComponent<Button>();

        var carImg = MakeImage(go.transform, "ThumbImage", Color.white);
        carImg.preserveAspect = true;
        SetAnchored(carImg.GetComponent<RectTransform>(),
            new Vector2(0, 0.3f), new Vector2(1, 1), new Vector2(4, -4), new Vector2(-4, 0));

        var nameT = MakeTMP(go.transform, "ThumbName", "",
            14, FontStyle.Bold, COL_TEXT_LIGHT);
        SetAnchored(nameT.GetComponent<RectTransform>(),
            new Vector2(0, 0), new Vector2(1, 0.3f), new Vector2(4, 2), new Vector2(-4, -2));
        nameT.alignment = TextAlignmentOptions.Center;
        nameT.enableWordWrapping = true;

        var rarity = MakeTMP(go.transform, "RarityBadge", "О",
            13, FontStyle.Bold, COL_GOLD);
        SetAnchored(rarity.GetComponent<RectTransform>(),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(22, 22));
        rarity.alignment = TextAlignmentOptions.Center;

        var thumb = go.AddComponent<CardThumbnailUI>();
        thumb.cardImage = carImg.GetComponent<Image>();
        thumb.nameLabel = nameT;
        thumb.rarityBadge = rarity;
        return go;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SHARED UI HELPERS
    // ══════════════════════════════════════════════════════════════════════
    static GameObject MakeCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        var evSys = new GameObject("EventSystem");
        evSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        return go;
    }

    static Image MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>(); img.color = color;
        return img;
    }

    static TextMeshProUGUI MakeTMP(Transform parent, string name, string text,
        int fontSize, FontStyle style, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fontSize; t.color = color;
        t.fontStyle = style == FontStyle.Bold ? FontStyles.Bold :
                      style == FontStyle.Italic ? FontStyles.Italic : FontStyles.Normal;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }

    static Button MakeMenuButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(320, 52);
        var img = go.AddComponent<Image>(); img.color = COL_BTN_GREEN;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = COL_BTN_GREEN;
        colors.highlightedColor = HEX("#388E3C");
        colors.pressedColor = COL_BTN_HOVER;
        btn.colors = colors;
        var lbl = MakeTMP(go.transform, "Label", label, 24, FontStyle.Bold, Color.white);
        StretchFull(lbl.GetComponent<RectTransform>());
        lbl.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    static Button MakeSmallButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchored(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        go.AddComponent<Image>().color = HEX("#455A64");
        var btn = go.AddComponent<Button>();
        var lbl = MakeTMP(go.transform, "Label", label, 18, FontStyle.Bold, Color.white);
        StretchFull(lbl.GetComponent<RectTransform>());
        lbl.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    static TMP_InputField MakeInputField(Transform parent, string name,
        string placeholder, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchored(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
        var inp = go.AddComponent<TMP_InputField>();

        var textArea = MakeEmptyRect(go.transform, "Text Area");
        StretchFull(textArea);
        textArea.gameObject.AddComponent<RectMask2D>();

        var ph = MakeTMP(textArea, "Placeholder", placeholder,
            20, FontStyle.Italic, new Color(0.5f, 0.5f, 0.5f));
        StretchFull(ph.GetComponent<RectTransform>());
        ph.margin = new Vector4(8, 4, 8, 4);

        var txt = MakeTMP(textArea, "Text", "", 20, FontStyle.Normal, Color.white);
        StretchFull(txt.GetComponent<RectTransform>());
        txt.margin = new Vector4(8, 4, 8, 4);

        inp.textViewport = textArea;
        inp.textComponent = txt;
        inp.placeholder = ph;
        inp.text = "";
        return inp;
    }

    static void MakeLabeledSlider(Transform parent, string name, string label, float y)
    {
        var lbl = MakeTMP(parent, $"Lbl_{name}", label, 22, FontStyle.Normal, COL_TEXT_LIGHT);
        SetAnchored(lbl.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-80, y), new Vector2(150, 30));

        var sliderGO = new GameObject($"{name}/Slider"); sliderGO.transform.SetParent(parent, false);
        var sliderRT = sliderGO.AddComponent<RectTransform>();
        SetAnchored(sliderRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(90, y), new Vector2(200, 22));
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0.7f;

        var bg = MakeImage(sliderGO.transform, "Background", new Color(0.25f, 0.25f, 0.35f));
        StretchFull(bg.GetComponent<RectTransform>());

        var fillArea = MakeEmptyRect(sliderGO.transform, "Fill Area");
        SetAnchored(fillArea, new Vector2(0, 0.25f), new Vector2(1, 0.75f),
            new Vector2(5, 0), new Vector2(-15, 0));
        var fill = MakeImage(fillArea, "Fill", COL_BTN_GREEN);
        StretchFull(fill.GetComponent<RectTransform>());

        var handleArea = MakeEmptyRect(sliderGO.transform, "Handle Slide Area");
        SetAnchored(handleArea, new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(10, 0), new Vector2(-10, 0));
        var handleImg = MakeImage(handleArea, "Handle", Color.white);
        handleImg.GetComponent<RectTransform>().sizeDelta = new Vector2(22, 22);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleImg.GetComponent<RectTransform>();
        slider.targetGraphic = handleImg.GetComponent<Image>();
    }

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchored(rt, anchorMin, anchorMax, pos, size);
        var img = go.AddComponent<Image>(); img.color = new Color(0.1f, 0.1f, 0.18f, 0.96f);
        return go;
    }

    // ── Layout helpers ─────────────────────────────────────────────────
    static RectTransform MakeEmptyRect(Transform parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    static void SetAnchored(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // ── Scene management ───────────────────────────────────────────────
    static Scene NewScene(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        return scene;
    }

    static void SaveScene(Scene scene, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        EditorSceneManager.SaveScene(scene, path);
    }

    static void SetBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",   true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity",  true),
            new EditorBuildSettingsScene("Assets/Scenes/GalleryScene.unity",true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("✅ Build Settings updated");
    }

    static void EnsureTMPImported()
    {
        if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
        {
            Debug.LogWarning("⚠ TextMeshPro not imported! " +
                "Go: Window → TextMeshPro → Import TMP Essential Resources");
        }
    }

    static Color HEX(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
#endif