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
        public int StackSize { get; set; } = 1;
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
        public float BonusCrit { get; set; }
        public float BonusBlock { get; set; }
        public int BonusEvasion { get; set; }
        public int BaseBonusATK { get; set; }
        public int BaseBonusDEF { get; set; }
        public int BaseBonusMATK { get; set; }
        public int BaseBonusMDEF { get; set; }
        public int BaseBonusAim { get; set; }
        public int BaseBonusEvasion { get; set; }
        public int BaseBonusSTR { get; set; }
        public int BaseBonusDEX { get; set; }
        public int BaseBonusEND { get; set; }
        public int BaseBonusINT { get; set; }
        public int BaseBonusSPR { get; set; }
        public float BaseBonusCrit { get; set; }
        public float BaseBonusBlock { get; set; }
        public int BaseBonusHP { get; set; }
        public int BaseBonusMP { get; set; }
        public int BonusHP { get; set; }
        public int BonusMP { get; set; }
    }

}
