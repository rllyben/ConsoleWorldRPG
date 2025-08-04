using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Commands;
using ConsoleWorldRPG.Services;

namespace ConsoleWorldRPG.Systems
{
    public class CommandRouter
    {
        private readonly Player _player;

        public CommandRouter(Player player)
        {
            _player = player;
        }

        public bool HandleCommand(string input)
        {
            input = input.Trim().ToLower();

            return
                InventoryCommands.Handle(input, _player) ||
                NpcCommands.Handle(input, _player) ||
                PlayerCommands.Handle(input, _player) ||
                MovementCommands.Handle(input, _player) ||
                CombatCommands.Handle(input, _player) ||
                LootCommands.Handle(input, _player) ||
                DebugCommands.Handle(input, _player) ||
                GatherCommands.Handle(input, _player) ||
                MapCommands.Handle(input, _player);
        }

    }

}
