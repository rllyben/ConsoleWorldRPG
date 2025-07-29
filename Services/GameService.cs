using System;
using System.Text.Json;
using System.Xml.Linq;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Interfaces;
using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG.Services
{
    public class GameService
    {
        private static Dictionary<int, Room> rooms = new();
        private static List<Monster> monster = new();
        public static bool InitializeGame(ref Player player, ref bool playerExitst)
        {
            NotifyUser("monster");
            LoadMonster();
            NotifyUser("rooms");
            bool success = LoadRooms();
            NotifyUser("connect monster to their rooms");
            ConnectMonsterRooms();
            NotifyUser("items");
            ItemFactory.LoadItems();
            NotifyUser("hero");
            playerExitst = LoadHero(ref player); 
            Console.WriteLine(player.WeaponSlot?.GetType().Name); // should be JsonEquipmentItem
            Console.WriteLine(player.WeaponSlot?.Name);           // should be Flamecaster's Staff
            NotifyUser("hero position");
            try
            {
                player.CurrentRoom = rooms[1];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error accured when setting the heros position: {ex.Message}\n Exiting...");
                return false;
            }

            Console.SetCursorPosition(0, Console.GetCursorPosition().Top + 1);
            Console.WriteLine();
            return success;
        }
        private static void LoadMonster()
        {
            monster = MonsterService.LoadMonsters();
        }
        private static bool LoadRooms()
        {
            rooms = RoomService.LoadRooms();
            if (rooms.Count == 0)
            {
                Console.WriteLine("No rooms found! Exiting...");
                return false;
            }
            return true;
        }
        private static void ConnectMonsterRooms()
        {
            foreach (Monster mob in monster)
            {
                var selectedRooms = rooms.Values.Where(r => r.HasMonsters && r.EncounterableMonsters.ContainsKey(mob.Id));
                foreach (Room room in selectedRooms)
                {
                    room.Monsters.Add(mob);
                }

            }

        }
        private static bool LoadHero(ref Player hero)
        {
            string filePath = $"player_hero.json";

            if (!File.Exists(filePath))
            {

            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new ItemConverter() }
                };
                string jsonData = File.ReadAllText(filePath);
                hero = JsonSerializer.Deserialize<Player>(jsonData, options);
            }
            catch 
            {
                return false;
            }
            return true;
        }
        private static void NotifyUser(string status)
        {
            Console.WriteLine($"Loading {status} ...");
        }
        /// <summary>
        /// Saves the current state of the hero
        /// </summary>
        public static void SaveHero(ref Player player)
        {
            player.CurrentRoomId = player.CurrentRoom.Id;
            string filePath = $"player_hero.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new ItemConverter() },
                PropertyNameCaseInsensitive = true
            };
            string jsonData = JsonSerializer.Serialize(player, options);
            File.WriteAllText(filePath, jsonData);
            Console.WriteLine($"Hero saved to {filePath}");
        }

    }

}