using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG.Commands
{
    public static class CombatCommands
    {
        /// <summary>
        /// Handels Combat commands
        /// </summary>
        /// <param name="input">player input</param>
        /// <param name="player">player character</param>
        /// <returns>if the command was found</returns>
        public static bool Handle(string input, Player player)
        {
            if (input == "fight")
            {
                if (player.CurrentRoom.Monsters.Any())
                {
                    CombatSystem.StartEncounter(player);
                }
                else
                {
                    Console.WriteLine("❌ There's nothing to fight here.");
                }
                return true;
            }

            return false;
        }

    }

}
