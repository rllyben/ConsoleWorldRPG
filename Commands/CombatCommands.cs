namespace ConsoleWorldRPG.Commands
{
    public static class CombatCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "fight")
            {
                if (player.CurrentRoom.Monsters.Any())
                    EncounterRunner.StartEncounter(player);
                else
                    Console.WriteLine("❌ There's nothing to fight here.");
                return true;
            }

            return false;
        }

    }

}
