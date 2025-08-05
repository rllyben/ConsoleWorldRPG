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
        /// <summary>
        /// Loads all game data
        /// </summary>
        /// <returns>if all loadings where successful</returns>
        public static bool InitializeGame()
        {
            bool success = false;
            try
            {
                NotifyUser("monster");
                monster = MonsterService.LoadMonsters();
                NotifyUser("rooms");
                success = LoadRooms();
                NotifyUser("connect monster to their rooms");
                ConnectMonsterRooms();
                NotifyUser("dungons");
                DungeonRegistry.Load();
                NotifyUser("caves");
                CaveRegistry.Load();
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
            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine("Exited with error: ", ex.ToString());
            }

            return success;
        }
        /// <summary>
        /// loads all rooms from RoomService
        /// </summary>
        /// <returns>returns if it was successful</returns>
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
        /// <summary>
        /// connects monsters to their saved rooms
        /// </summary>
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
        /// <summary>
        /// prints an user notification what is loaded currently
        /// </summary>
        /// <param name="status">the info whats loaded</param>
        private static void NotifyUser(string status)
        {
            Console.WriteLine($"Loading {status} ...");
        }

    }

}