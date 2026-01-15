using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Systems
{
    public static class Localization
    {
        private static Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);
        public static CultureInfo Culture { get; private set; } = CultureInfo.InvariantCulture;

        public static void Load(GameLanguage lang)
        {
            var file = lang switch
            {
                GameLanguage.DE => "Data/misc/locales/de.json",
                //GameLanguage.Fr => "Data/locales/fr.json",
                //GameLanguage.Es => "Data/locales/es.json",
                _ => "Data/misc/locales/en.json"
            };
            if (!File.Exists(file)) file = "Data/misc/locales/en.json";

            var json = File.ReadAllText(file);
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Culture = lang switch
            {
                GameLanguage.DE => new CultureInfo("de-DE"),
                //GameLanguage.Fr => new CultureInfo("fr-FR"),
                //GameLanguage.Es => new CultureInfo("es-ES"),
                _ => new CultureInfo("en-US")
            };

        }

        public static string T(string key, params object[] args)
        {
            if (!_strings.TryGetValue(key, out var format))
                return $"[{key}]"; // visible fallback so missing keys are easy to spot

            return args is { Length: > 0 }
                ? string.Format(Culture, format, args)
                : format;
        }

    }

}
