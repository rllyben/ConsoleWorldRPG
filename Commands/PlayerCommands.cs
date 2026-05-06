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
                Console.WriteLine("The 'heal' command is no longer available. Use a potion or visit a healer.");
                return true;
            }
            else if (input == "logout")
            {
                CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                Console.Clear();
                Game.LogOut = true;
                return true;
            }

            return false;
        }

    }

}
