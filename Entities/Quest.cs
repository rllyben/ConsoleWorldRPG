using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Entities
{
    public class Quest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string GiverNpc { get; set; }
        public int RequiredLevel { get; set; } = 1;
        public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
        public Dictionary<int, int> RequiredKills { get; set; } = new(); // monsterId => amount
        public Dictionary<int, int> KillProgress { get; set; } = new();  // monsterId => current count
        public Dictionary<string, int> RequiredItems { get; set; } = new();   // itemId => amount
        public Dictionary<string, int> ItemProgress { get; set; } = new();    // itemId => how many the player has

        public int RewardXp { get; set; }
        public int RewardGold { get; set; }
        public List<string> RewardItems { get; set; } = new();
    }

}
