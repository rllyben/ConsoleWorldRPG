using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG.Services
{
    public static class GameStatusService
    {
        private static string path = Path.Combine("Data", "gameStatus.json");

        public static GameStatus Load()
        {
            if (!File.Exists(path))
                return new GameStatus { LastUpdateTime = DateTime.Now };

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameStatus>(json);
        }

        public static void Save(GameStatus status)
        {
            var json = JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

    }

}
