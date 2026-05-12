namespace ConsoleWorldRPG.Services
{
    public static class GameService
    {
        public static IReadOnlyDictionary<int, Room> Rooms => MyriaLib.Services.GameService.Rooms;

        public static bool InitializeGame(Player player)
        {
            try
            {
                SettingsService.Load();
                Localization.Load(Settings.Current.LanguageSettings.Local);

                var progress = new Progress<string>(step =>
                    Console.WriteLine($"Loading {step}..."));

                MyriaLib.Services.GameService.InitializeGame(progress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialization failed: {ex.Message}");
                return false;
            }

            return true;
        }

    }

}
