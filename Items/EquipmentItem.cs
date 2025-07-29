using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Items
{
    public class EquipmentItem : Item
    {
        public List<PlayerClass> AllowedClasses { get; set; } = new(); 
        public EquipmentType SlotType { get; set; }
        public override int BuyPrice { get; set; } = 300; // base cost

        // Core stat bonuses (used by rare/special gear)
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
        public float BonusCrit { get; set; }
        public float BonusBlock { get; set; }

        public override void Use(Player player)
        {
            Console.WriteLine($"{Name} is a piece of equipment and cannot be used directly.");
        }

        public bool IsUsableBy(Player player) => AllowedClasses.Contains(player.Class);
    }

}
