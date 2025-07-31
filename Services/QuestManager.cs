using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Services
{
    public static class QuestManager
    {
        private static List<Quest> _allQuests = new();

        public static void LoadQuests(string path = "Data/quests.json")
        {
            var json = File.ReadAllText(path);
            _allQuests = JsonSerializer.Deserialize<List<Quest>>(json)!;
        }
        public static List<Quest> GetAvailableForPlayer(Player player)
        {
            return _allQuests
                .Where(q =>
                    player.Level >= q.RequiredLevel &&
                    !player.ActiveQuests.Any(aq => aq.Id == q.Id) &&
                    !player.CompletedQuests.Any(cq => cq.Id == q.Id))
                .ToList();
        }

        public static Quest? GetQuestById(string id) =>
            _allQuests.FirstOrDefault(q => q.Id == id);
    }

}
