using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Utils;

namespace ConsoleWorldRPG.Commands
{
    public static class PlayerCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "status")
            {
                Printer.ShowStatus(player);
                return true;
            }
            else if (input == "help")
            {
                Printer.ShowHelp();
                return true;
            }
            else if (input == "heal")
            {
                //Console.WriteLine("You take a break and heal all your wounds");
                //_player.CurrentHealth = _player.MaxHealth;
                Console.WriteLine("The 'heal' command is no longer available. Use a potion or visit a healer.");
                return true;
            }

                return false;
        }

    }

}
