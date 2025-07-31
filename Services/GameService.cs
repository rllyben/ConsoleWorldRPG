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
        public static Dictionary<int, Room> rooms = new();
        private static List<Monster> monster = new();
        public static bool InitializeGame()
        {
            NotifyUser("monster");
            LoadMonster();
            NotifyUser("rooms");
            bool success = LoadRooms();
            NotifyUser("connect monster to their rooms");
            ConnectMonsterRooms();
            NotifyUser("items");
            ItemFactory.LoadItems();
            NotifyUser("quests");
            QuestManager.LoadQuests();
            NotifyUser("skills");
            SkillFactory.LoadSkills();
            NotifyUser("Day cycle"); 
            DayCycleManager.Initialize();
            DayCycleManager.StartBackgroundLoop();
            Console.WriteLine();
            return success;
        }
        private static void LoadMonster()
        {
            monster = MonsterService.LoadMonsters(); 
            foreach (var monster in monster)
            {
                Console.WriteLine($"{monster.Name} => {monster.Type} ({(int)monster.Type})");
            }
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
        private static void NotifyUser(string status)
        {
            Console.WriteLine($"Loading {status} ...");
        }

    }

}