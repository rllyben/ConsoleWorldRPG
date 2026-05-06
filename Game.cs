using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG
{
    public class Game
    {
        private bool _isRunning = true;
        private Player _player = new Player("Hero", new Stats());
        private CommandRouter _commandRouter;
        public static bool LogOut = false;

        public void Start()
        {
            Console.WriteLine(Localization.T("app.startmassage"));
            if (GameService.InitializeGame(_player))
            {
                while (_isRunning)
                {
                    LogOut = false;
                    _player = ShowLoginMenu();
                    if (_player == null)
                        _isRunning = false;

                    if (_isRunning)
                    {
                        _commandRouter = new CommandRouter(_player);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(Localization.T("app.helpinfo"));
                        Console.ResetColor();
                        _player.CurrentRoom.Describe();
                    }

                    while (_isRunning)
                    {
                        Console.Write("> ");
                        string? input = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(input)) continue;

                        if (input == "exit")
                        {
                            CharacterService.SaveCharacter(LoginManager.UserAccount, _player);
                            _isRunning = false;
                            Console.WriteLine(Localization.T("app.goodbye"));
                            break;
                        }

                        if (LogOut)
                            break;
                        else if (!_commandRouter.HandleCommand(input))
                            Console.WriteLine(Localization.T("ui_unknown_command"));
                    }
                }
            }
        }

        public static Player ShowLoginMenu()
        {
            while (true)
            {
                Console.WriteLine("\n==== " + Localization.T("ui_login_menue") + " ====");
                Console.WriteLine("0. " + Localization.T("ui_exit"));
                Console.WriteLine("1. " + Localization.T("ui_login"));
                Console.WriteLine("2. " + Localization.T("ui_register"));
                Console.WriteLine("3. " + Localization.T("ui_settings"));
                Console.Write(Localization.T("ui_select_option"));

                string? input = Console.ReadLine()?.Trim();
                switch (input)
                {
                    case "0": return null;
                    case "1":
                        var user = LoginManager.Login();
                        if (user != null) return user;
                        break;
                    case "2":
                        LoginManager.Register();
                        break;
                    case "3":
                        ShowSettingsMenu();
                        break;
                    default:
                        Console.WriteLine(Localization.T("ui_invalid_option"));
                        break;
                }
            }
        }

        private static void ShowSettingsMenu()
        {
            Console.WriteLine("\n== " + Localization.T("ui_settings") + " ==");
            Console.WriteLine("1. " + Localization.T("ui_language") + " (" + Settings.Current.LanguageSettings.Local + ")");
            Console.WriteLine("0. " + Localization.T("ui_back"));
            Console.Write("> ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.WriteLine(Localization.T("ui_select_language") + "1) En  2) De");
                Console.Write("> ");
                var l = Console.ReadLine();
                Settings.Current.LanguageSettings.Local = l switch
                {
                    "2" => GameLanguage.De,
                    _   => GameLanguage.En
                };
                SettingsService.Save();
                Localization.Load(Settings.Current.LanguageSettings.Local);
                Console.WriteLine(Localization.T("msg.language_applied", Settings.Current.LanguageSettings.Local));
            }
        }

    }

}
