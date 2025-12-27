using System;
using System.Collections.Generic;

namespace LanguageTrainer
{
    public static class FlagManager
    {
        private static Dictionary<string, string> flagUrls = new Dictionary<string, string>
        {
            {"Русский", "https://flagcdn.com/w40/ru.webp"},
            {"Английский", "https://flagcdn.com/w40/gb.webp"},
            {"Китайский", "https://flagcdn.com/w40/cn.webp"},
            {"Испанский", "https://flagcdn.com/w40/es.webp"},
            {"Арабский", "https://flagcdn.com/w40/ae.webp"},
            {"Хинди", "https://flagcdn.com/w40/in.webp"},
            {"Бенгальский", "https://flagcdn.com/w40/bd.webp"},
            {"Португальский", "https://flagcdn.com/w40/pt.webp"},
            {"Индонезийский", "https://flagcdn.com/w40/id.webp"},
            {"Урду", "https://flagcdn.com/w40/pk.webp"},
            {"Немецкий", "https://flagcdn.com/w40/de.webp"},
            {"Японский", "https://flagcdn.com/w40/jp.webp"},
            {"Суахили", "https://flagcdn.com/w40/tz.webp"},
            {"Маратхи", "https://flagcdn.com/w40/in.webp"},
            {"Телугу", "https://flagcdn.com/w40/in.webp"},
            {"Турецкий", "https://flagcdn.com/w40/tr.webp"},
            {"Корейский", "https://flagcdn.com/w40/kr.webp"},
            {"Французский", "https://flagcdn.com/w40/fr.webp"},
            {"Итальянский", "https://flagcdn.com/w40/it.webp"},
            {"Вьетнамский", "https://flagcdn.com/w40/vn.webp"}
        };

        public static string GetFlagUrl(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Flag_of_the_United_Nations.svg/320px-Flag_of_the_United_Nations.svg.png";

            if (flagUrls.TryGetValue(name, out string url))
            {
                return url;
            }

            foreach (KeyValuePair<string, string> pair in flagUrls)
            {
                if (name.IndexOf(pair.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return pair.Value;
                }
            }

            return "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Flag_of_the_United_Nations.svg/320px-Flag_of_the_United_Nations.svg.png";
        }
    }
}