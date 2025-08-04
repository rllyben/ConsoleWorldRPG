using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Items
{
    public abstract class ConsumableItem : Item
    {
        public int HealAmount { get; set; }
        public int ManaRestore { get; set; }
        /// <summary>
        /// uses the item and grants its effects
        /// </summary>
        /// <param name="player">player character</param>
        public override void Use(Player player)
        {
            if (HealAmount > 0)
            {
                int healed = Math.Min(HealAmount, player.Stats.MaxHealth - player.CurrentHealth);
                player.CurrentHealth += healed;
                Console.WriteLine($"You recover {healed} HP!");
            }

            if (ManaRestore > 0)
            {
                int restored = Math.Min(ManaRestore, player.Stats.MaxMana - player.CurrentMana);
                player.CurrentMana += restored;
                Console.WriteLine($"You recover {restored} Mana!");
            }

        }

    }

}
