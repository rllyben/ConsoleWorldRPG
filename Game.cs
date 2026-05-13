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

            MyriaLib.Services.GameService.SessionStarted += _ => DayCycleManager.StartInactivityTimer();
            MyriaLib.Services.GameService.SessionStarted += p => ClassManager.ApplyDailyPenalty(p);
            MyriaLib.Services.GameService.SessionStarted += p =>
            {
                // J18: knowledge gather bonus for already-rolled room limits
                int bonus = JobManager.GetGatherKnowledgeBonus(p);
                if (bonus > 0)
                    foreach (var room in RoomService.AllRooms.Where(r => r.GatheringSpots.Count > 0))
                        room.AddGatherBonus(bonus);

                // J11–J14: daily job ticks; J18: re-apply on each new day
                DayCycleManager.DayAdvanced += day =>
                {
                    JobManager.ApplyDailyTicks(p, day);
                    int b = JobManager.GetGatherKnowledgeBonus(p);
                    if (b > 0)
                        foreach (var r in RoomService.AllRooms.Where(r => r.GatheringSpots.Count > 0))
                            r.AddGatherBonus(b);
                };
            };

            // Hub events — subscribed once; fire from background threads while hub is connected
            ConsoleHubClient.ChatMessageReceived += (sender, msg, channel) =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n  [{channel}] {sender}: {msg}");
                Console.ResetColor();
            };
            ConsoleHubClient.PlayerEntered += name =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"\n  ** {name} entered the room.");
                Console.ResetColor();
            };
            ConsoleHubClient.PlayerLeft += name =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n  ** {name} left the room.");
                Console.ResetColor();
            };
            ConsoleHubClient.RoomPlayersReceived += players =>
            {
                if (players.Count == 0) return;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n  ** Also here: {string.Join(", ", players)}");
                Console.ResetColor();
            };
            ConsoleHubClient.PartyInviteReceived += (from, partyId) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  ** {from} invited you to a party.  Type: party accept {partyId}");
                Console.ResetColor();
            };
            ConsoleHubClient.PartyUpdated += (members, leader) =>
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n  ** Party: [{string.Join(", ", members)}]  Leader: {leader ?? "(none)"}");
                Console.ResetColor();
            };
            ConsoleHubClient.PartyDisbanded += () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n  ** Your party has been disbanded.");
                Console.ResetColor();
            };
            ConsoleHubClient.KickedFromParty += () =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  ** You have been kicked from the party.");
                Console.ResetColor();
            };

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
                        MyriaLib.Services.GameService.StartSession(_player);
                        _ = TryConnectHubAsync(_player);

                        _commandRouter = new CommandRouter(_player);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(Localization.T("app.helpinfo"));
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"  [Day {DayCycleManager.GameDay} — {DayCycleManager.CurrentTimeSegment}]");
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
                            SavePlayer(_player);
                            DayCycleManager.StopInactivityTimer();
                            ConsoleHubClient.DisconnectAsync().GetAwaiter().GetResult();
                            _isRunning = false;
                            Console.WriteLine(Localization.T("app.goodbye"));
                            break;
                        }

                        if (LogOut)
                        {
                            DayCycleManager.StopInactivityTimer();
                            ConsoleHubClient.DisconnectAsync().GetAwaiter().GetResult();
                            break;
                        }
                        else if (!_commandRouter.HandleCommand(input))
                            Console.WriteLine(Localization.T("ui_unknown_command"));
                    }
                }
            }
        }

        private static void SavePlayer(Player player)
        {
            if (ConsoleHubClient.HasToken)
            {
                bool ok = ConsoleHubClient.SaveCharacterAsync(player).GetAwaiter().GetResult();
                if (!ok)
                {
                    Console.WriteLine("Warning: Server save failed. Saving locally...");
                    CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                }
            }
            else
            {
                CharacterService.SaveCharacter(LoginManager.UserAccount, player);
            }
        }

        private static async Task TryConnectHubAsync(Player player)
        {
            if (!ConsoleHubClient.HasToken)
            {
                // REST login may have been skipped if server was offline during local auth — try once more
                bool ok = await ConsoleHubClient.LoginAsync(
                    LoginManager.UserAccount.Username,
                    LoginManager.UserAccount.Password);
                if (!ok) return;
            }

            await ConsoleHubClient.ConnectAsync();

            if (ConsoleHubClient.IsConnected)
            {
                await ConsoleHubClient.SetCharacterNameAsync(player.Name);
                await ConsoleHubClient.JoinRoomAsync(player.CurrentRoomId);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  [Online — connected to multiplayer server]");
                Console.ResetColor();
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
