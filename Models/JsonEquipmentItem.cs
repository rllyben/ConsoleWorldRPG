using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Models
{
    public class JsonEquipmentItem : EquipmentItem
    {
        public JsonEquipmentItem(GameItem def)
        {
            Id = def.Id;
            Name = def.Name;
            Description = def.Description;
            StackSize = 1;
            BuyPrice = def.BuyPrice;
            SlotType = Enum.Parse<EquipmentType>(def.SlotType.ToString());
            AllowedClasses = def.AllowedClasses?
                .Select(c => Enum.Parse<PlayerClass>(c)).ToList() ?? new();

            BaseBonusSTR = def.BonusSTR;
            BaseBonusDEX = def.BonusDEX;
            BaseBonusEND = def.BonusEND;
            BaseBonusINT = def.BonusINT;
            BaseBonusSPR = def.BonusSPR;

            BaseBonusATK = def.BonusATK;
            BaseBonusDEF = def.BonusDEF;
            BaseBonusMATK = def.BonusMATK;
            BaseBonusMDEF = def.BonusMDEF;
            BaseBonusAim = def.BonusAim;
            BaseBonusEvasion = def.BonusEvasion;

            // Core stat bonuses
            BonusSTR = def.BonusSTR;
            BonusDEX = def.BonusDEX;
            BonusEND = def.BonusEND;
            BonusINT = def.BonusINT;
            BonusSPR = def.BonusSPR;

            // Derived stat bonuses
            BonusATK = def.BonusATK;
            BonusDEF = def.BonusDEF;
            BonusMATK = def.BonusMATK;
            BonusMDEF = def.BonusMDEF;
            BonusAim = def.BonusAim;
            BonusEvasion = def.BonusEvasion;
        }

    }

}
