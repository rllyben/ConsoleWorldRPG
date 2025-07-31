using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Models;

namespace ConsoleWorldRPG.Services
{
    public class LoginManager
    {
        public static UserAccount UserAccount { get; private set; }
        public static void Register()
        {
            Console.Write("Choose a username: ");
            string username = Console.ReadLine()?.Trim().ToLower();

            string path = Path.Combine("Data/users", $"{username}.json");
            if (File.Exists(path))
            {
                Console.WriteLine("❌ That username already exists.");
                return;
            }

            Console.Write("Choose a password: ");
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

            Console.WriteLine("✅ Account created.");
        }
        public static Player Login()
        {
            bool success = false;
            UserAccount user = null;
            while (!success)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine()?.Trim().ToLower();
                string path = Path.Combine("Data/users", $"{username}.json");

                if (!File.Exists(path))
                {
                    Console.WriteLine("❌ No such user.");
                    continue;
                }

                Console.Write("Password: ");
                string password = Console.ReadLine()?.Trim();

                var json = File.ReadAllText(path);
                user = JsonSerializer.Deserialize<UserAccount>(json);

                if (user!.Password != password)
                {
                    Console.WriteLine("❌ Incorrect password.");
                    continue;
                }
                success = true;
            }
            UserAccount = user;

            while (true)
            {
                Console.WriteLine("\nChoose a character:");
                for (int i = 0; i < user.CharacterNames.Count; i++)
                    Console.WriteLine($"{i + 1}. {user.CharacterNames[i]}");

                if (user.CharacterNames.Count < 5)
                    Console.WriteLine("C. Create new character");

                Console.Write("Select (number or C): ");
                string input = Console.ReadLine()!.Trim();

                if (input.ToLower() == "c" && user.CharacterNames.Count < 5)
                {
                    Console.Write("Enter new character name: ");
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
            Console.WriteLine("no player found!");
            Console.WriteLine();
            Console.Write("Choose hero name:");
            player.Name = Console.ReadLine();
            Console.WriteLine($"Hero name is set to {player.Name}");
            Console.WriteLine();
            Console.WriteLine("Choose an hero class");
            Console.WriteLine("Classes:");
            foreach (PlayerClass klasse in ClassProfile.All.Keys)
            {
                Console.WriteLine(klasse);
            }
            Console.Write("> ");
            string input = Console.ReadLine()?.Trim().ToLower();
            HandleClassSelection(input, ref player);
            Console.WriteLine($"Hero Class is set to {player.Class}");
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
