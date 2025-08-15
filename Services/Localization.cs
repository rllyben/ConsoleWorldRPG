using System.Globalization;
using System.Text.Json;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Services
{
    public static class Localization
    {
        private static Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);
        public static CultureInfo Culture { get; private set; } = CultureInfo.InvariantCulture;

        public static void Load(GameLanguage lang)
        {
            string file = lang switch
            {
                GameLanguage.DE => "Data/misc/locales/de.json",
                _ => "Data/misc/locales/en.json"
            };

            if (!File.Exists(file)) file = "Data/misc/locales/en.json";

            var json = File.ReadAllText(file);
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Culture = lang switch
            {
                GameLanguage.DE => new CultureInfo("de-DE"),
                _ => new CultureInfo("en-US")
            };

        }

        public static string T(string key, params object[] args)
        {
            if (!_strings.TryGetValue(key, out var format))
                return $"[{key}]";

            return args.Length > 0
                ? string.Format(Culture, format, args)
                : format;
        }

    }

}