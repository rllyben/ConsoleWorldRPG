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
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Skills;

namespace ConsoleWorldRPG.Entities
{
    public class Player : CombatEntity
    {
        public PlayerClass Class { get; set; } = PlayerClass.Fighter;
        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public long ExpForNextLvl { get; set; }
        public int PotionTierAvailable { get; set; } = 1;
        public Inventory Inventory { get; set; } = new();
        public MoneyBag Money { get; set; } = new(); 
        public List<Skill> Skills => SkillFactory.GetSkillsFor(this); 
        public List<Quest> ActiveQuests { get; set; } = new();
        public List<Quest> CompletedQuests { get; set; } = new();

        [JsonIgnore]
        public Room CurrentRoom { get; set; }
        public int CurrentRoomId { get; set; }
        public int? LastHealerRoomId { get; set; } = null;

        // Add inventory, experience, commands, etc.
        public Player(string name, Stats stats)
        {
            Name = name;
            Stats = stats;
            CurrentHealth = stats.MaxHealth;
            CurrentMana = stats.MaxMana;
            ExpForNextLvl = (long)(Math.Pow(Level, 2)) * 50;
        }
        /// <summary>
        /// Handels level ups
        /// </summary>
        public void CheckForLevelup()
        {
            while (ExpForNextLvl < Experience)
            {
                LevelUp();
                Experience -= ExpForNextLvl;
                ExpForNextLvl = (long)(Math.Pow(Level, 2)) * 50;
            }

        }
        /// <summary>
        /// Applies an death penalty if the player dies
        /// </summary>
        public void ApplyDeathXpPenalty()
        {
            long penalty = (long)(ExpForNextLvl * 0.01f);
            long actualLoss = Math.Min(penalty, Experience);
            Experience -= actualLoss;

            Console.WriteLine($"🩸 You lost {actualLoss} XP as a penalty for dying.");
        }
        /// <summary>
        /// equips an equipment item to its slot
        /// </summary>
        /// <param name="item">item to equip</param>
        public void Equip(EquipmentItem item)
        {
            switch (item.SlotType)
            {
                case EquipmentType.Weapon:
                    if (WeaponSlot != null) Inventory.AddItem(WeaponSlot, this);
                    WeaponSlot = item;
                    break;
                case EquipmentType.Armor:
                    if (ArmorSlot != null) Inventory.AddItem(ArmorSlot, this);
                    ArmorSlot = item;
                    break;
                case EquipmentType.Accessory:
                    if (AccessorySlot != null) Inventory.AddItem(AccessorySlot, this);
                    AccessorySlot = item;
                    break;
            }

            Console.WriteLine($"Equipped: {item.Name}");
        }
        /// <summary>
        /// updates stats for an Level up
        /// </summary>
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
        /// <summary>
        /// returns the bonus from equiped gear
        /// </summary>
        /// <param name="selector">function to select from</param>
        /// <returns>the bonmus as an int</returns>
        public int GetBonusFromGear(Func<EquipmentItem, int> selector)
        {
            int total = 0;
            if (WeaponSlot != null) total += selector(WeaponSlot);
            if (ArmorSlot != null) total += selector(ArmorSlot);
            if (AccessorySlot != null) total += selector(AccessorySlot);
            return total;
        }
        /// <summary>
        /// Checks if the player has gather equipment
        /// </summary>
        /// <param name="type">athering type of the source</param>
        /// <returns>if the player can gather</returns>
        public bool HasToolFor(GatheringType type)
        {
            return type switch
            {
                GatheringType.Ore => Inventory.Items.Any(i => i.Id == "pickaxe"),
                GatheringType.Tree => Inventory.Items.Any(i => i.Id == "woodcutter_axe"),
                GatheringType.Herb => true, // no tool required
                _ => false
            };

        }

    }

}
