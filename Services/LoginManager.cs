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
            }
            UserAccount = user;

            while (true)
            {
                Console.WriteLine("\n" + Localization.T("ui_character_menue"));
                for (int i = 0; i < user.CharacterNames.Count; i++)
                    Console.WriteLine($"{i + 1}. {user.CharacterNames[i]}");

                if (user.CharacterNames.Count < 5)
                    Console.WriteLine(Localization.T("ui_character_create"));

                Console.Write(Localization.T("ui_character_selection"));
                string input = Console.ReadLine()!.Trim();

                if (input.ToLower() == "c" && user.CharacterNames.Count < 5)
                {
                    Console.Write(Localization.T("ui_character_creation_name"));
                    string charName = Console.ReadLine()!.Trim();
                    user.CharacterNames.Add(charName);

                    File.WriteAllText(
                        Path.Combine("Data/users", $"{user.Username}.json"),
                        JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true }));

                    return CreateNewCharacter(charName);
                }

                if (int.TryParse(input, out int idx) && idx > 0 && idx <= user.CharacterNames.Count)
                    return CharacterService.LoadCharacter(user.CharacterNames[idx - 1], user);
            }
        }

        private static Player CreateNewCharacter(string name)
        {
            var player = new Player(name, new Stats());
            Console.WriteLine(Localization.T("ui_creation_name_set") + player.Name + "\n");
            Console.WriteLine(Localization.T("ui_creation_choose_class"));
            Console.WriteLine(Localization.T("ui_creation_classes"));
            foreach (PlayerClass klasse in ClassProfile.All.Keys)
                Console.WriteLine(klasse);

            Console.Write("> ");
            string input = Console.ReadLine()?.Trim().ToLower() ?? "";
            player.Class = input switch
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

            Console.WriteLine(Localization.T("ui_creation_class_set") + player.Class);
            player.CurrentRoom = RoomService.AllRooms.FirstOrDefault(r => r.Id == 1);
            return player;
        }

    }

}
