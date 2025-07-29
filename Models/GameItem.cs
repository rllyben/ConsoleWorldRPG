using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Models
{
    public class GameItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; } = "Common"; // read from JSON
        public int BuyPrice { get; set; }
        public int MaxStackSize { get; set; } = 1;
        public List<string> AllowedClasses { get; set; } = new();
        public string Type { get; set; } // "consumable", "equipment", etc.
        public int HealAmount { get; set; }
        public int ManaRestore { get; set; }
        public EquipmentType SlotType { get; set; }

        // Core stat bonuses
        public int BonusSTR { get; set; }
        public int BonusDEX { get; set; }
        public int BonusEND { get; set; }
        public int BonusINT { get; set; }
        public int BonusSPR { get; set; }

        // Derived stat bonuses
        public int BonusATK { get; set; }
        public int BonusDEF { get; set; }
        public int BonusMATK { get; set; }
        public int BonusMDEF { get; set; }
        public int BonusAim { get; set; }
        public int BonusEvasion { get; set; }
    }

}
