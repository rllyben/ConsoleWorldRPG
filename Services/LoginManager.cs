using System.Text.Json;

namespace ConsoleWorldRPG.Services
{
    public static class LoginManager
    {
        public static UserAccount UserAccount { get; private set; }

        public static void Register()
        {
            Console.Write(Localization.T("ui_register_username"));
            string username = Console.ReadLine()?.Trim().ToLower() ?? "";

            string path = Path.Combine("Data/users", $"{username}.json");
            if (File.Exists(path))
            {
                Console.WriteLine(Localization.T("ui_register_user_exitsts"));
                return;
            }

            Console.Write(Localization.T("ui_register_password"));
            string password = Console.ReadLine()?.Trim() ?? "";

            var account = new UserAccount { Username = username, Password = password };

            Directory.CreateDirectory("Data/users");
            var json = JsonSerializer.Serialize(account, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            Console.WriteLine(Localization.T("ui_register_successful"));
        }

        public static Player Login()
        {
            bool success = false;
            UserAccount user = null;
            while (!success)
            {
                Console.Write(Localization.T("ui_login_username") + ": ");
                string username = Console.ReadLine()?.Trim().ToLower() ?? "";
                string path = Path.Combine("Data/users", $"{username}.json");

                if (!File.Exists(path))
                {
                    Console.WriteLine(Localization.T("ui_login_no_user"));
                    continue;
                }

                Console.Write(Localization.T("ui_login_password") + ": ");
                string password = Console.ReadLine()?.Trim() ?? "";

                var json = File.ReadAllText(path);
                user = JsonSerializer.Deserialize<UserAccount>(json);

                if (user!.Password != password)
                {
                    Console.WriteLine(Localization.T("ui_login_wrong_password"));
                    continue;
                }
                success = true;

                // Attempt server REST login immediately so character list/load can use server
                ConsoleHubClient.LoginAsync(username, password).GetAwaiter().GetResult();
            }
            UserAccount = user;

            // Character list: server when online, local otherwise
            List<string> charNames;
            if (ConsoleHubClient.HasToken)
            {
                charNames = ConsoleHubClient.GetCharacterNamesAsync().GetAwaiter().GetResult();
                if (charNames.Count == 0 && user.CharacterNames.Count > 0)
                    charNames = user.CharacterNames; // fall back if server returned empty unexpectedly
            }
            else
            {
                charNames = user.CharacterNames;
            }

            while (true)
            {
                Console.WriteLine("\n" + Localization.T("ui_character_menue"));
                for (int i = 0; i < charNames.Count; i++)
                    Console.WriteLine($"{i + 1}. {charNames[i]}");

                bool canCreate = charNames.Count < 5;
                if (canCreate)
                    Console.WriteLine(Localization.T("ui_character_create"));

                Console.Write(Localization.T("ui_character_selection"));
                string input = Console.ReadLine()!.Trim();

                if (input.ToLower() == "c" && canCreate)
                {
                    Console.Write(Localization.T("ui_character_creation_name"));
                    string charName = Console.ReadLine()!.Trim();

                    var newPlayer = CreateNewCharacter(charName);

                    if (ConsoleHubClient.HasToken)
                    {
                        // Server only: save to server, don't update local user file
                        bool saved = ConsoleHubClient.SaveCharacterAsync(newPlayer).GetAwaiter().GetResult();
                        if (!saved)
                        {
                            Console.WriteLine("Warning: Server save failed. Saving locally...");
                            user.CharacterNames.Add(charName);
                            File.WriteAllText(
                                Path.Combine("Data/users", $"{user.Username}.json"),
                                JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true }));
                            CharacterService.SaveCharacter(user, newPlayer);
                        }
                    }
                    else
                    {
                        user.CharacterNames.Add(charName);
                        File.WriteAllText(
                            Path.Combine("Data/users", $"{user.Username}.json"),
                            JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true }));
                        CharacterService.SaveCharacter(user, newPlayer);
                    }

                    return newPlayer;
                }

                if (int.TryParse(input, out int idx) && idx > 0 && idx <= charNames.Count)
                {
                    string selectedName = charNames[idx - 1];
                    if (ConsoleHubClient.HasToken)
                    {
                        var serverPlayer = ConsoleHubClient.LoadCharacterAsync(selectedName).GetAwaiter().GetResult();
                        if (serverPlayer != null) return serverPlayer;
                        Console.WriteLine("Warning: Server load failed. Loading locally...");
                    }
                    return CharacterService.LoadCharacter(selectedName, user);
                }
            }
        }

        private static Player CreateNewCharacter(string name)
        {
            var player = new Player(name, new Stats());
            Console.WriteLine(Localization.T("ui_creation_name_set") + player.Name + "\n");

            // ── Race selection ────────────────────────────────────────────────
            var allRaces = RaceProfile.All;
            var raceList = allRaces.Keys.ToList();

            Console.WriteLine(Localization.T("ui_creation_choose_race"));
            Console.WriteLine(new string('─', 60));
            for (int i = 0; i < raceList.Count; i++)
            {
                var race = raceList[i];
                var rp   = allRaces[race];
                var b    = rp.BaseStatBonus;
                var g    = rp.StatGrowth;
                Console.WriteLine($"{i + 1}. {race}");
                Console.WriteLine($"   {Localization.T($"race.{race}.desc")}");
                Console.WriteLine($"   {Localization.T("ui_creation_race_stats")}: STR {10 + b["STR"],2}  DEX {10 + b["DEX"],2}  END {10 + b["END"],2}  INT {10 + b["INT"],2}  SPR {10 + b["SPR"],2}  HP +{rp.BaseHpBonus}  MP +{rp.BaseManaBonus}");
                Console.WriteLine($"   {Localization.T("ui_creation_race_growth")}: STR +{g["STR"]}  DEX +{g["DEX"]}  END +{g["END"]}  INT +{g["INT"]}  SPR +{g["SPR"]}  HP +{rp.HpPerLevel}  MP +{rp.ManaPerLevel}");
                Console.WriteLine();
            }

            PlayerRace selectedRace = PlayerRace.Myralu;
            while (true)
            {
                Console.Write(Localization.T("ui_creation_select_race", raceList.Count));
                string raceInput = Console.ReadLine()?.Trim() ?? "";
                if (int.TryParse(raceInput, out int raceIdx) && raceIdx >= 1 && raceIdx <= raceList.Count)
                {
                    selectedRace = raceList[raceIdx - 1];
                    break;
                }
                Console.WriteLine(Localization.T("ui_invalid_option"));
            }

            var raceProfile = allRaces[selectedRace];
            player.Race               = selectedRace;
            player.RaceSelected       = true;
            player.Stats.Strength     = 10 + raceProfile.BaseStatBonus["STR"];
            player.Stats.Dexterity    = 10 + raceProfile.BaseStatBonus["DEX"];
            player.Stats.Endurance    = 10 + raceProfile.BaseStatBonus["END"];
            player.Stats.Intelligence = 10 + raceProfile.BaseStatBonus["INT"];
            player.Stats.Spirit       = 10 + raceProfile.BaseStatBonus["SPR"];
            player.Stats.BaseHealth  += raceProfile.BaseHpBonus;
            player.Stats.BaseMana    += raceProfile.BaseManaBonus;
            Console.WriteLine(Localization.T("ui_creation_race_set") + selectedRace + "\n");

            // ── Class selection ───────────────────────────────────────────────
            var allowedClasses = ClassProfile.All.Keys
                .Where(c => !raceProfile.ForbiddenClasses.Contains(c))
                .ToList();

            Console.WriteLine(Localization.T("ui_creation_choose_class"));
            Console.WriteLine(Localization.T("ui_creation_classes"));
            foreach (var klasse in allowedClasses)
                Console.WriteLine(klasse);

            Console.Write("> ");
            string classInput = Console.ReadLine()?.Trim().ToLower() ?? "";
            var chosen = classInput switch
            {
                "archer"        => PlayerClass.Archer,
                "arcanmage"     => PlayerClass.ArcanMage,
                "barbarian"     => PlayerClass.Barbarian,
                "cleric"        => PlayerClass.Cleric,
                "druid"         => PlayerClass.Druid,
                "elementalmage" => PlayerClass.ElementalMage,
                "fighter"       => PlayerClass.Fighter,
                "hunter"        => PlayerClass.Hunter,
                "knight"        => PlayerClass.Knight,
                "rogue"         => PlayerClass.Rogue,
                "soulsknight"   => PlayerClass.SoulsKnight,
                _               => PlayerClass.Fighter
            };
            player.Class = allowedClasses.Contains(chosen) ? chosen : allowedClasses[0];

            Console.WriteLine(Localization.T("ui_creation_class_set") + player.Class);
            player.CurrentRoom = RoomService.AllRooms.FirstOrDefault(r => r.Id == 1);
            return player;
        }

    }

}
