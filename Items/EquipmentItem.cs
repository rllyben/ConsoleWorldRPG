using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        public int BaseBonusHP { get; set; }
        public int BaseBonusMP { get; set; }
        public int BaseBonusATK { get; set; }
        public int BaseBonusDEF { get; set; }
        public int BaseBonusMATK { get; set; }
        public int BaseBonusMDEF { get; set; }
        public int BaseBonusAim { get; set; }
        public int BaseBonusEvasion { get; set; }
        public float BaseBonusCrit { get; set; }
        public float BaseBonusBlock { get; set; }
        public int BaseBonusSTR { get; set; }
        public int BaseBonusDEX { get; set; }
        public int BaseBonusEND { get; set; }
        public int BaseBonusINT { get; set; }
        public int BaseBonusSPR { get; set; }

        // Core stat bonuses (used by rare/special gear)
        public int BonusHP { get; set; }
        public int BonusMP { get; set; }
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
        public int UpgradeLevel { get; set; } = 0;

        public bool IsUsableBy(Player player) => AllowedClasses.Contains(player.Class);
        public override void Use(Player player)
        {
            Console.WriteLine($"{Name} is a piece of equipment and cannot be used directly.");
        }
        /// <summary>
        /// tries to upgrade the item
        /// </summary>
        /// <param name="player">player character</param>
        /// <returns>retruns if its successful</returns>
        public bool TryUpgrade(Player player)
        {
            if (UpgradeLevel >= 9)
            {
                Console.WriteLine("🔒 This item is already at the maximum upgrade level (+9).");
                return false;
            }

            string requiredItemId = "upgrade_stone"; // later: different items per tier
            var material = player.Inventory.Items
                .FirstOrDefault(i => i.Id == requiredItemId);

            if (material == null)
            {
                Console.WriteLine($"❌ You need an {requiredItemId.Replace("_", " ")} to upgrade this item.");
                return false;
            }
            if (material.StackSize < 2)
                player.Inventory.RemoveItem(material);
            else
                material.StackSize--;

            UpgradeLevel++;
            float multiplier = 1.1f;
            if (UpgradeLevel < 4)
                multiplier = 1 + (UpgradeLevel * 0.1f);
            else if (UpgradeLevel < 7)
                multiplier = 1 + (UpgradeLevel * 0.3f);
            else if (UpgradeLevel < 10)
                multiplier = 1 + (UpgradeLevel * 0.7f);

            BonusATK = (int)(BaseBonusATK * multiplier);
            BonusDEF = (int)(BaseBonusDEF * multiplier);
            BonusMATK = (int)(BaseBonusMATK * multiplier);
            BonusMDEF = (int)(BaseBonusMDEF * multiplier);
            BonusAim = (int)(BaseBonusAim * multiplier);
            BonusEvasion = (int)(BaseBonusEvasion * multiplier);
            BonusSTR = (int)(BaseBonusSTR * multiplier);
            BonusDEX = (int)(BaseBonusDEX * multiplier);
            BonusEND = (int)(BaseBonusEND * multiplier);
            BonusINT = (int)(BaseBonusINT * multiplier);
            BonusSPR = (int)(BaseBonusSPR * multiplier);
            BonusHP = (int)(BaseBonusHP * multiplier);
            BonusMP = (int)(BaseBonusMP * multiplier);

            return true;
        }

    }

}
