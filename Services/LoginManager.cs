using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Models;
using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG.Services
{
    public class LoginManager
    {
        public static UserAccount UserAccount { get; private set; }
        public static void Register()
        {
            Console.Write(Localization.T("ui_register_username"));
            string username = Console.ReadLine()?.Trim().ToLower();

            string path = Path.Combine("Data/users", $"{username}.json");
            if (File.Exists(path))
            {
                Console.WriteLine(Localization.T("ui_register_user_exitsts"));
                return;
            }

            Console.Write(Localization.T("ui_register_password"));
            string password = Console.ReadLine()?.Trim();

            var account = new UserAccount
            {
                Username = username,
                Password = password
            };

            if (!Path.Exists(path))
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
                string username = Console.ReadLine()?.Trim().ToLower();
                string path = Path.Combine("Data/users", $"{username}.json");

                if (!File.Exists(path))
                {
                    Console.WriteLine(Localization.T("ui_login_no_user"));
                    continue;
                }

                Console.Write(Localization.T("ui_login_password") + ": ");
                string password = Console.ReadLine()?.Trim();

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

                if (int.TryParse(input, out int idx) &&
                    idx > 0 && idx <= user.CharacterNames.Count)
                {
                    return JsonSaveService.LoadCharacter(user.CharacterNames[idx - 1], user);
                }

            }

        }
        private static Player CreateNewCharacter(string name)
        {
            Player player = new Player("null", new Stats());
            player.Name = name;
            Console.WriteLine(Localization.T("ui_creation_name_set") + player.Name + "\n");
            Console.WriteLine(Localization.T("ui_creation_choose_class")); 
            Console.WriteLine(Localization.T("ui_creation_classes"));
            foreach (PlayerClass klasse in ClassProfile.All.Keys)
            {
                Console.WriteLine(klasse);
            }
            Console.Write("> ");
            string input = Console.ReadLine()?.Trim().ToLower();
            HandleClassSelection(input, ref player);
            Console.WriteLine(Localization.T("ui_creation_class_set") + player.Class);
            player.CurrentRoom = RoomService.AllRooms.FirstOrDefault(r => r.Id == 1);
            return player;
        }
        /// <summary>
        /// Handles the Class selection of the Player
        /// </summary>
        /// <param name="input"></param>
        private static void HandleClassSelection(string input, ref Player player)
        {
            switch (input)
            {
                case "archer": player.Class = PlayerClass.Archer; break;
                case "arcanmage": player.Class = PlayerClass.ArcanMage; break;
                case "barbarian": player.Class = PlayerClass.Barbarian; break;
                case "cleric": player.Class = PlayerClass.Cleric; break;
                case "druid": player.Class = PlayerClass.Druid; break;
                case "elementalmage": player.Class = PlayerClass.ElementalMage; break;
                case "fighter": player.Class = PlayerClass.Fighter; break;
                case "hunter": player.Class = PlayerClass.Hunter; break;
                case "knight": player.Class = PlayerClass.Knight; break;
                case "rogue": player.Class = PlayerClass.Rogue; break;
                case "soulsknight": player.Class = PlayerClass.SoulsKnight; break;
            }

        }

    }

}
