# 🏎️ GonkiCards — Быстрый старт

## Установка за 5 шагов

### 1. Установи Unity
- Скачай [Unity Hub](https://unity.com/download)
- Установи **Unity 2022.3 LTS** (или новее)

### 2. Создай новый проект
```
Unity Hub → New Project → 2D → назови "GonkiCards" → Create
```

### 3. Распакуй и скопируй файлы
Из этого архива скопируй папки прямо в `Assets/` твоего проекта:
```
GonkiCards/Assets/Scripts/    →    [ТвойПроект]/Assets/Scripts/
GonkiCards/Assets/Resources/  →    [ТвойПроект]/Assets/Resources/
```
Подожди пока Unity скомпилирует (~10 сек).

### 4. Импортируй TextMeshPro
```
Window → TextMeshPro → Import TMP Essential Resources → Import All
```
⚠️ БЕЗ ЭТОГО ВСЁ СЛОМАЕТСЯ

### 5. Запусти построитель сцен
```
Верхнее меню Unity → GonkiCards → Build All Scenes
```
Нажми OK в диалоге. Сцены созданы!

### 6. Играй!
```
Верхнее меню → GonkiCards → Build All Scenes (если ещё не нажал)
Открой сцену: Assets/Scenes/MainMenu.unity
Нажми ▶ Play
```

---

## Что делает "Build All Scenes"

Скрипт `GonkiCardsBuilder.cs` автоматически:
- Создаёт `MainMenu.unity` с главным меню, настройками, мультиплеером
- Создаёт `GameScene.unity` с полным игровым столом
- Создаёт `GalleryScene.unity` с галереей всех 7 карт
- Создаёт префаб CardThumbnail
- Добавляет все сцены в Build Settings

---

## Карточки в игре

| # | Машина | Мощность | Макс. скорость | 0-100 | Объём | Масса |
|---|--------|----------|----------------|-------|-------|-------|
| 7 | Subaru BRZ | 235 л.с. | 226 км/ч | 6.8 с | 2387 | 1290 кг |
| 8 | Skoda Superb | 220 л.с. | 247 км/ч | 7.0 с | 1984 | 1510 кг |
| 9 | Jeep Grand Cherokee | 190 л.с. | 189 км/ч | 10.9 с | 3964 | 1860 кг |
| 10 | Lada 2106 | 75 л.с. | 155 км/ч | 16.0 с | 1569 | 1050 кг |
| 11 | Aston Martin Valour | 715 л.с. | 322 км/ч | 3.5 с | 5204 | 1850 кг |
| 12 | Ford GT40 | 485 л.с. | 330 км/ч | 4.3 с | 6997 | 1048 кг |
| 13 | ЗАЗ 965 | 27 л.с. | 90 км/ч | — | 887 | 650 кг |

**Больше лучше:** Мощность, Макс. скорость, Объём двигателя  
**Меньше лучше:** 0-100, Масса

---

## Структура файлов

```
Assets/
├── Scripts/
│   ├── Editor/
│   │   └── GonkiCardsBuilder.cs   ← строит все сцены
│   ├── Data/
│   │   ├── CardData.cs
│   │   ├── CardsManager.cs        ← все 7 машин
│   │   └── PlayerData.cs
│   ├── Game/
│   │   └── GameManager.cs         ← вся логика + BotAI
│   ├── UI/
│   │   ├── GameSceneController.cs
│   │   ├── CardDisplayUI.cs
│   │   ├── StatButtonUI.cs
│   │   ├── PlayerSlotUI.cs
│   │   ├── RoundResultPanel.cs
│   │   ├── GameOverScreen.cs
│   │   ├── MainMenuController.cs
│   │   ├── GalleryController.cs
│   │   └── ...ещё 6 файлов
│   └── Utils/
│       ├── AudioManager.cs
│       ├── Localization.cs        ← RU / EN
│       └── SceneBootstrapper.cs
└── Resources/
    └── Cards/
        └── 7.png … 13.png        ← карточки
```

---

## Если что-то не работает

| Ошибка | Решение |
|--------|---------|
| `TMPro not found` | Window → TextMeshPro → Import TMP Essential Resources |
| Меню GonkiCards не появилось | Подожди компиляцию, проверь ошибки в Console |
| Карты не отображаются | Убедись что PNG в `Assets/Resources/Cards/` |
| Сцены не открываются | File → Open Scene → Assets/Scenes/MainMenu.unity |

---

Удачи! 🏎️💨
