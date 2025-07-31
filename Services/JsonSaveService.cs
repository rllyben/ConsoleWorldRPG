using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Models;
using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG.Services
{
    public static class JsonSaveService
    {
        public static void SaveCharacter(UserAccount user, Player player)
        {
            string path = Path.Combine("Data/saves", $"{user.Username}-{player.Name}.json");

            if (!Path.Exists(path))
                Directory.CreateDirectory("Data/saves");

            player.CurrentRoomId = player.CurrentRoom.Id;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new ItemConverter() }  // ✅ this line is crucial
            };
            var json = JsonSerializer.Serialize(player, options);
            File.WriteAllText(path, json);
        }

        public static Player LoadCharacter(string name, UserAccount user)
        {
            string path = Path.Combine("Data/saves", $"{user.Username}-{name}.json");
            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new ItemConverter() }  // ✅ this line is crucial
            };

            var jsonHero = JsonSerializer.Deserialize<Player>(json, options);
            Player player = jsonHero;
            try
            {
                int roomId = player.CurrentRoomId;
                player.CurrentRoom = RoomService.AllRooms.FirstOrDefault(r => r.Id == roomId);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error accured when setting the heros position: {ex.Message}!");
                Console.ResetColor();
                return player;
            }
            SkillFactory.UpdateSkills(ref player);
            return player;
        }

    }

}
