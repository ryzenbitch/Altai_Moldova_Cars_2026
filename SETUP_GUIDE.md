# 🏎️ GonkiCards — Unity Setup Guide

## Требования
- Unity 2022.3 LTS (или 2023+)
- TextMeshPro (установить через Package Manager)
- Создай проект: **2D (URP)** или просто **2D**

---

## Шаг 1 — Структура проекта

Создай папки в Assets:
```
Assets/
├── Scripts/          ← все .cs файлы из проекта
│   ├── Core/
│   ├── Data/
│   ├── Game/
│   ├── UI/
│   └── Utils/
├── Resources/
│   └── Cards/        ← положи сюда 7.png … 13.png (ОБЯЗАТЕЛЬНО)
├── Sprites/
│   ├── UI/
│   └── Avatars/
├── Prefabs/
├── Scenes/
└── Audio/
```

> ⚠️ **ВАЖНО**: Карточки (7.png–13.png) должны лежать именно в `Assets/Resources/Cards/`
> потому что код грузит их через `Resources.Load<Sprite>("Cards/7")`.

---

## Шаг 2 — Сцены

Создай 3 сцены: `MainMenu`, `GameScene`, `GalleryScene`
Добавь их в **File → Build Settings** именно в таком порядке (индексы 0, 1, 2).

---

## Шаг 3 — MainMenu сцена

### Иерархия объектов:
```
MainMenu (Scene)
├── [Bootstrapper] — пустой GameObject
│     └── SceneBootstrapper.cs
├── Canvas (Screen Space - Overlay, CanvasScaler: 1920×1080)
│   ├── Background — Image (тёмно-зелёный #1B2A1E или твой фон)
│   ├── Logo — Image/TextMeshProUGUI "GONKI CARDS"
│   ├── MainMenuButtons — VerticalLayoutGroup
│   │   ├── btn_play_bot     — Button + TMP "Играть с ботом"
│   │   ├── btn_play_friend  — Button + TMP "Играть с другом"
│   │   ├── btn_gallery      — Button + TMP "Галерея"
│   │   ├── btn_settings     — Button + TMP "Настройки"
│   │   └── btn_exit         — Button + TMP "Выход"
│   ├── SettingsPanel (изначально неактивен)
│   │   ├── Title — TMP "Настройки"
│   │   ├── MusicSlider — Slider (0–1)
│   │   ├── SFXSlider   — Slider (0–1)
│   │   ├── NicknameInput — TMP_InputField
│   │   ├── AvatarButton — Button
│   │   ├── LangRU_Button, LangEN_Button
│   │   └── CloseButton
│   └── MultiplayerSetupPanel (изначально неактивен)
│       ├── TwoPlayersBtn, ThreePlayersBtn
│       ├── PlayerName_0, PlayerName_1, PlayerName_2 — TMP_InputField
│       ├── AvatarBtn_0, AvatarBtn_1, AvatarBtn_2
│       ├── StartButton
│       └── CloseButton
└── AudioSource (BGMusic, loop)
└── AudioSource (SFX)
```

### Привяжи MainMenuController.cs:
- Создай пустой GameObject `[MainMenuController]`
- Добавь компонент `MainMenuController`
- Перетащи все кнопки и панели в соответствующие поля

---

## Шаг 4 — GameScene сцена

### Иерархия:
```
GameScene
├── [GameManager] — пустой GO
│     ├── GameManager.cs
├── [Bootstrapper]
│     └── SceneBootstrapper.cs
├── [GameSceneController]
│     └── GameSceneController.cs
├── Canvas
│   ├── Background — Image (тёмно-зелёный стол)
│   │
│   ├── ── ИГРОВОЙ СТОЛ ──
│   │
│   ├── OpponentSlot_1 (верхний левый)
│   │   ├── PlayerSlotUI.cs
│   │   ├── AvatarImage, NicknameText, DeckCount, ReserveCount
│   │   └── CardDisplayArea
│   │       └── CardDisplayUI.cs + все дочерние элементы
│   │
│   ├── OpponentSlot_2 (верхний правый)   ← только при 3 игроках
│   │   └── (аналогично)
│   │
│   ├── MySlot (нижний центр)
│   │   ├── PlayerSlotUI.cs
│   │   ├── AvatarImage, NicknameText
│   │   ├── DeckCounterUI.cs
│   │   │   ├── DeckCount TMP (показывает "В колоде: X")
│   │   │   └── ReserveCount TMP (показывает "В резерве: X")
│   │   └── MyCardDisplay — CardDisplayUI.cs
│   │
│   ├── ── HUD ──
│   ├── TurnBannerText — TMP
│   ├── MyTurnBanner — панель "ВАШ ХОД!" (скрыта по умолчанию)
│   ├── RoundCounterText — TMP "Раунд X"
│   │
│   ├── ── ВЫБОР ХАРАКТЕРИСТИКИ ──
│   ├── StatPanel (скрыт по умолчанию)
│   │   ├── Title "Выберите характеристику:"
│   │   └── 5× StatButton (StatButtonUI.cs)
│   │       каждая кнопка: StatNameText, StatValueText, BetterHintText
│   │
│   ├── ── РЕЗУЛЬТАТ РАУНДА ──
│   ├── RoundResultPanel (RoundResultPanel.cs, скрыт)
│   │   ├── SelectedStatText
│   │   ├── RoundResultHeadline
│   │   ├── 3× CardDisplayUI (для каждого игрока)
│   │   ├── 3× PlayerNameText
│   │   ├── 3× WinnerCrown (иконка короны)
│   │   └── ContinueButton
│   │
│   ├── EliminationNotice (скрыт)
│   │   └── EliminationText TMP
│   │
│   └── GameOverScreen (GameOverScreen.cs, скрыт)
│       ├── HeadlineText, WinnerNameText, WinnerAvatarImage
│       ├── StatsText
│       ├── PlayAgainButton, MainMenuButton
│       └── ParticleSystem (конфетти)
```

### Структура CardDisplayUI (для КАЖДОЙ карточки):
```
CardRoot
├── CardBack (показывается когда карта скрыта)
│   └── Image (рубашка карты)
└── CardFront (показывается когда карта раскрыта)
    ├── CardBackground — Image (фоновый цвет карты)
    ├── CarImage — Image (фото машины)
    ├── CardIdText — TMP (номер: "7", "8"...)
    ├── RarityText — TMP ("О" или "Р")
    ├── CarNameText — TMP
    ├── CarModelText — TMP
    ├── DescriptionText — TMP
    ├── PriceText — TMP
    └── StatsContainer
        ├── AccelerationText — TMP  ← stat[0]
        ├── HorsepowerText   — TMP  ← stat[1]
        ├── MaxSpeedText     — TMP  ← stat[2]
        ├── EngineVolumeText — TMP  ← stat[3]
        ├── WeightText       — TMP  ← stat[4]
        └── 5× StatHighlight — Image (прозрачные по умолчанию)
            (располагаются поверх каждого стата)
```

---

## Шаг 5 — GalleryScene сцена

```
GalleryScene
├── [Bootstrapper]
├── Canvas
│   ├── Title — TMP "Галерея автомобилей"
│   ├── BackButton
│   ├── ScrollView
│   │   └── Viewport → Content (GridLayoutGroup)
│   │       ← сюда динамически спавнятся превью карточек
│   └── DetailPanel (GalleryController: detailPanel, скрыт)
│       ├── DetailCarImage
│       ├── DetailCarName, DetailCarModel
│       ├── DetailDescription
│       ├── DetailRarity, DetailPrice
│       ├── Stat Labels (acceleration, hp, speed, volume, weight)
│       ├── PrevButton, NextButton
│       └── CloseDetailButton
└── [GalleryController] — GalleryController.cs
    └── создай prefab CardThumbnail:
        ├── Image (карточка, 547×768 уменьшенная)
        ├── TextMeshProUGUI (название)
        └── Button компонент
```

---

## Шаг 6 — Prefab CardThumbnail

1. Создай `GameObject` → `UI → Image`
2. Размер: 160×225 (или похожее)
3. Добавь дочерний `TextMeshProUGUI` для имени (внизу карты)
4. Добавь `Button` компонент на корень
5. Сохрани как Prefab в `Assets/Prefabs/CardThumbnail.prefab`
6. Назначь в `GalleryController → cardThumbnailPrefab`

---

## Шаг 7 — Настройки карточных изображений

В Unity выбери каждый PNG из `Resources/Cards/`:
- **Texture Type**: Sprite (2D and UI)
- **Pixels Per Unit**: 100
- **Filter Mode**: Bilinear
- **Compression**: None (или Quality)
- Нажми **Apply**

---

## Шаг 8 — TextMeshPro

При первом использовании TMP Unity предложит импортировать Essential Resources — **соглашайся**!
Для русского языка используй шрифт с кириллицей. Рекомендую:
1. Скачай **Roboto** или **Noto Sans** с поддержкой кириллицы
2. В Unity: `Window → TextMeshPro → Font Asset Creator`
3. Создай TMP FontAsset с кириллицей (Unicode range: `0020-007E,0400-04FF`)
4. Назначь как Default Font в TMP Settings

---

## Шаг 9 — Аудио (опционально)

Добавь AudioSource компоненты или используй AudioManager.
Рекомендуемые бесплатные звуки с freesound.org:
- Перемешивание карт
- Щелчок карты
- Кнопка UI
- Победа/поражение (fanfare/sad trombone 😄)

---

## Шаг 10 — Запуск

1. Открой сцену `MainMenu`
2. Убедись что все 3 сцены в Build Settings
3. Нажми Play → "Играть с ботом"
4. Игра должна запуститься!

---

## Цветовая палитра (из скриншота)

| Элемент          | HEX         |
|------------------|-------------|
| Фон стола        | `#3D5040`   |
| Коричневый (HUD) | `#5C3317`   |
| Карта (фиолет.)  | `#8B3F8C`   |
| Карта (красная)  | `#C0392B`   |
| Карта (синяя)    | `#1A5BA6`   |
| Карта (оранжевая)| `#D4820A`   |
| Карта (зелёная)  | `#1E7A2F`   |
| Золото (победа)  | `#FFD700`   |
| Текст светлый    | `#F0EAD6`   |
| Статы (дисплей)  | `#D4C87A`   |

---

## Краткое резюме логики игры

```
StartGame()
  └─ Раздать карты (6 каждому)
  └─ Случайный первый ход

Каждый раунд:
  1. PlayerTurn → DrawTopCard → Show card
  2. SelectingStat (человек выбирает, бот авто)
  3. RevealingCards → Все вскрывают карты
  4. ResolvingRound → Определяем победителя
     - acceleration & weight: МЕНЬШЕ лучше
     - horsepower, maxSpeed, engineVolume: БОЛЬШЕ лучше
  5. Победитель забирает все карты в резерв
  6. Если у кого-то 0 карт → выбывает
  7. Если 1 игрок остался → GameOver
  8. Иначе → следующий ход по часовой стрелке
```

---

## Файлы скриптов

| Файл | Назначение |
|------|-----------|
| `CardData.cs` | Модель данных карточки + StatType |
| `CardsManager.cs` | База всех 7 карт, создание колоды |
| `PlayerData.cs` | Модель игрока + GameSettings |
| `GameManager.cs` | Вся игровая логика + BotAI |
| `GameSceneController.cs` | Главный UI-контроллер игровой сцены |
| `GameUIController.cs` | Альтернативный UI-контроллер |
| `CardDisplayUI.cs` | Отображение одной карточки |
| `CardFlipAnimation.cs` | Анимация переворота карты |
| `CardDealAnimator.cs` | Анимация раздачи карт |
| `StatButtonUI.cs` | Кнопка выбора характеристики |
| `PlayerSlotUI.cs` | Слот игрока за столом |
| `DeckCounterUI.cs` | Счётчик карт в колоде/резерве |
| `RoundResultPanel.cs` | Панель результата раунда |
| `GameOverScreen.cs` | Экран конца игры |
| `MainMenuController.cs` | Главное меню |
| `GalleryController.cs` | Галерея карточек |
| `CardThumbnailUI.cs` | Превью карточки в галерее |
| `AvatarPickerUI.cs` | Выбор аватарки |
| `Localization.cs` | RU/EN переводы |
| `AudioManager.cs` | Музыка и звуки |
| `SceneBootstrapper.cs` | Инициализация синглтонов |
