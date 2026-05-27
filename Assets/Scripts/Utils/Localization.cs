using UnityEngine;
using System.Collections.Generic;

public static class Localization
{
    private static Dictionary<string, Dictionary<string, string>> strings
        = new Dictionary<string, Dictionary<string, string>>();

    static Localization()
    {
        // ---- RUSSIAN ----
        var ru = new Dictionary<string, string>
        {
            // Main Menu
            {"btn_play_bot",        "Играть с ботом"},
            {"btn_play_friend",     "Играть с другом"},
            {"btn_gallery",         "Галерея"},
            {"btn_settings",        "Настройки"},
            {"btn_exit",            "Выход"},

            // Settings
            {"settings_title",      "Настройки"},
            {"settings_music",      "Музыка"},
            {"settings_sfx",        "Звуки"},
            {"settings_nickname",   "Никнейм"},
            {"settings_avatar",     "Аватар"},
            {"settings_language",   "Язык"},
            {"btn_close",           "Закрыть"},
            {"btn_save",            "Сохранить"},

            // Multiplayer setup
            {"multi_how_many",      "Сколько игроков?"},
            {"multi_2players",      "2 игрока"},
            {"multi_3players",      "3 игрока"},
            {"multi_enter_name",    "Введите имя"},
            {"multi_choose_avatar", "Выбрать аватар"},
            {"btn_start",           "Начать игру!"},

            // Game
            {"your_turn",           "Ваш ход!"},
            {"choose_stat",         "Выберите характеристику:"},
            {"stat_accel",          "0-100 (сек)"},
            {"stat_hp",             "Мощность (л.с.)"},
            {"stat_speed",          "Макс. скорость (км/ч)"},
            {"stat_volume",         "Объём двигателя (см³)"},
            {"stat_weight",         "Масса (кг)"},
            {"less_better",         "↓ меньше — лучше"},
            {"more_better",         "↑ больше — лучше"},
            {"round_win",           "🏆 Вы победили в раунде!"},
            {"round_lose",          "Победил: {0}"},
            {"deck_label",          "В колоде"},
            {"reserve_label",       "В резерве"},
            {"eliminated",          "{0} выбывает из игры!"},
            {"turn_indicator",      "Ходит: {0}"},
            {"round_label",         "Раунд {0}"},

            // Game Over
            {"gameover_win",        "🎉 ВЫ ПОБЕДИЛИ!"},
            {"gameover_lose",       "Победитель: {0}"},
            {"btn_play_again",      "Играть снова"},
            {"btn_main_menu",       "Главное меню"},

            // Gallery
            {"gallery_title",       "Галерея автомобилей"},
            {"gallery_rarity",      "Редкость: {0}"},
            {"gallery_price",       "Цена: $ {0}"},

            // Card Stats
            {"card_accel",         "0-100: {0} сек"},
            {"card_hp",            "Мощность: {0} л.с."},
            {"card_speed",         "Макс. скорость: {0} км/ч"},
            {"card_volume",        "Объём: {0} см³"},
            {"card_weight",        "Масса: {0} кг"},
        };

        // ---- ENGLISH ----
        var en = new Dictionary<string, string>
        {
            {"btn_play_bot",        "Play vs Bot"},
            {"btn_play_friend",     "Play with Friend"},
            {"btn_gallery",         "Gallery"},
            {"btn_settings",        "Settings"},
            {"btn_exit",            "Exit"},

            {"settings_title",      "Settings"},
            {"settings_music",      "Music"},
            {"settings_sfx",        "Sound FX"},
            {"settings_nickname",   "Nickname"},
            {"settings_avatar",     "Avatar"},
            {"settings_language",   "Language"},
            {"btn_close",           "Close"},
            {"btn_save",            "Save"},

            {"multi_how_many",      "How many players?"},
            {"multi_2players",      "2 players"},
            {"multi_3players",      "3 players"},
            {"multi_enter_name",    "Enter name"},
            {"multi_choose_avatar", "Choose avatar"},
            {"btn_start",           "Start game!"},

            {"your_turn",           "Your turn!"},
            {"choose_stat",         "Choose a stat:"},
            {"stat_accel",          "0-100 (sec)"},
            {"stat_hp",             "Horsepower (hp)"},
            {"stat_speed",          "Top speed (km/h)"},
            {"stat_volume",         "Engine volume (cm³)"},
            {"stat_weight",         "Weight (kg)"},
            {"less_better",         "↓ lower is better"},
            {"more_better",         "↑ higher is better"},
            {"round_win",           "🏆 You won the round!"},
            {"round_lose",          "Winner: {0}"},
            {"deck_label",          "In deck"},
            {"reserve_label",       "Reserve"},
            {"eliminated",          "{0} is eliminated!"},
            {"turn_indicator",      "Turn: {0}"},
            {"round_label",         "Round {0}"},

            {"gameover_win",        "🎉 YOU WIN!"},
            {"gameover_lose",       "Winner: {0}"},
            {"btn_play_again",      "Play again"},
            {"btn_main_menu",       "Main menu"},

            {"gallery_title",       "Car Gallery"},
            {"gallery_rarity",      "Rarity: {0}"},
            {"gallery_price",       "Price: $ {0}"},

            {"card_accel",          "0-100: {0} sec"},
            {"card_hp",             "Power: {0} hp"},
            {"card_speed",          "Top speed: {0} km/h"},
            {"card_volume",         "Engine: {0} cm³"},
            {"card_weight",         "Weight: {0} kg"},
        };

        strings["RU"] = ru;
        strings["EN"] = en;
    }

    public static string Get(string key, params object[] args)
    {
        string lang = GameSettings.Language;
        if (!strings.ContainsKey(lang)) lang = "RU";

        if (strings[lang].TryGetValue(key, out string val))
        {
            if (args != null && args.Length > 0)
                return string.Format(val, args);
            return val;
        }

        // Fallback to RU
        if (lang != "RU" && strings["RU"].TryGetValue(key, out string fallback))
        {
            if (args != null && args.Length > 0)
                return string.Format(fallback, args);
            return fallback;
        }

        return $"[{key}]";
    }
}
