using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Entities
{
    public class Player : CombatEntity
    {
        public PlayerClass Class { get; set; } = PlayerClass.Fighter;
        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public long ExpForNextLvl { get; private set; }
        public int PotionTierAvailable { get; set; } = 1;
        public Inventory Inventory { get; set; } = new();
        public MoneyBag Money { get; set; } = new();
        public EquipmentItem? WeaponSlot { get; set; }
        public EquipmentItem? ArmorSlot { get; set; }
        public EquipmentItem? AccessorySlot { get; set; }

        [JsonIgnore]
        public Room CurrentRoom { get; set; }
        public int CurrentRoomId { get; set; }

        // Add inventory, experience, commands, etc.
        public Player(string name, Stats stats)
        {
            Name = name;
            Stats = stats;
            CurrentHealth = stats.MaxHealth;
            CurrentMana = stats.MaxMana;
            ExpForNextLvl = (long)(Math.Pow(Level, 2)) * 50;
        }

        public void ShowStatus()
        {
            Console.WriteLine($"{Name}'s Health: {CurrentHealth}/{MaxHealth}");
        }
        public void CheckForLevelup()
        {
            while (ExpForNextLvl < Experience)
            {
                LevelUp();
                Experience -= ExpForNextLvl;
                ExpForNextLvl = (long)(Math.Pow(Level, 2)) * 50;
            }

        }
        public void Equip(EquipmentItem item)
        {
            switch (item.SlotType)
            {
                case EquipmentType.Weapon:
                    WeaponSlot = item;
                    break;
                case EquipmentType.Armor:
                    ArmorSlot = item;
                    break;
                case EquipmentType.Accessory:
                    AccessorySlot = item;
                    break;
            }

            Console.WriteLine($"Equipped: {item.Name}");
        }
        public void LevelUp()
        {
            var profile = ClassProfile.All[Class];

            Level++;
            Stats.Strength += profile.StatGrowth["STR"];
            Stats.Dexterity += profile.StatGrowth["DEX"];
            Stats.Endurance += profile.StatGrowth["END"];
            Stats.Intelligence += profile.StatGrowth["INT"];
            Stats.Spirit += profile.StatGrowth["SPR"];

            Stats.BaseHealth += profile.HpPerLevel;
            Stats.BaseMana += profile.ManaPerLevel;

            CurrentHealth = Stats.MaxHealth;
            CurrentMana = Stats.MaxMana;

            Console.WriteLine($"\n🎉 You reached level {Level}!");
            Console.WriteLine($"HP: {Stats.MaxHealth}, Mana: {Stats.MaxMana}");
        }
        public int GetBonusFromGear(Func<EquipmentItem, int> selector)
        {
            int total = 0;
            if (WeaponSlot != null) total += selector(WeaponSlot);
            if (ArmorSlot != null) total += selector(ArmorSlot);
            if (AccessorySlot != null) total += selector(AccessorySlot);
            return total;
        }

    }

}
