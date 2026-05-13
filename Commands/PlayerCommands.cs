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
            else if (input == "character" || input == "stats")
            {
                Printer.ShowCharacter(player);
                return true;
            }
            else if (input.StartsWith("alloc "))
            {
                AllocStat(input[6..].Trim(), player);
                return true;
            }
            else if (input == "jobs")
            {
                Printer.ShowJobs(player);
                return true;
            }
            else if (input.StartsWith("say "))
            {
                string msg = input[4..].Trim();
                if (!ConsoleHubClient.IsConnected)
                    Console.WriteLine("Not connected to server. (offline mode)");
                else
                    _ = ConsoleHubClient.SendMessageAsync(msg, "room");
                return true;
            }
            else if (input.StartsWith("g "))
            {
                string msg = input[2..].Trim();
                if (!ConsoleHubClient.IsConnected)
                    Console.WriteLine("Not connected to server. (offline mode)");
                else
                    _ = ConsoleHubClient.SendMessageAsync(msg, "global");
                return true;
            }
            else if (input.StartsWith("whisper ") || input.StartsWith("w "))
            {
                int prefixLen = input.StartsWith("w ") ? 2 : 8;
                string rest   = input[prefixLen..].Trim();
                int space = rest.IndexOf(' ');
                if (space < 1) { Console.WriteLine("Usage: w <player> <message>"); return true; }
                string target = rest[..space];
                string msg    = rest[(space + 1)..].Trim();
                if (!ConsoleHubClient.IsConnected)
                    Console.WriteLine("Not connected to server. (offline mode)");
                else
                    _ = ConsoleHubClient.SendMessageAsync(msg, "private", target);
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
                if (ConsoleHubClient.HasToken)
                {
                    bool ok = ConsoleHubClient.SaveCharacterAsync(player).GetAwaiter().GetResult();
                    if (!ok)
                    {
                        Console.WriteLine("Warning: Server save failed. Saving locally...");
                        CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                    }
                }
                else
                {
                    CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                }
                Console.Clear();
                Game.LogOut = true;
                return true;
            }

            return false;
        }

        private static void AllocStat(string args, Player player)
        {
            string[] parts = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                Console.WriteLine("Usage: alloc <stat> [n]  (stat: str, dex, end, int, spr)");
                return;
            }
            string statKey = parts[0].ToUpper();
            int amount = parts.Length > 1 && int.TryParse(parts[1], out int n) ? n : 1;
            amount = Math.Min(amount, player.Stats.UnusedPoints);
            if (amount <= 0)
            {
                Console.WriteLine("No unspent stat points available.");
                return;
            }
            switch (statKey)
            {
                case "STR": player.Stats.StrengthBonus     += amount; break;
                case "DEX": player.Stats.DexterityBonus    += amount; break;
                case "END": player.Stats.EnduranceBonus    += amount; break;
                case "INT": player.Stats.IntelligenceBonus += amount; break;
                case "SPR": player.Stats.SpiritBonus       += amount; break;
                default:
                    Console.WriteLine($"Unknown stat '{statKey}'. Valid: str, dex, end, int, spr");
                    return;
            }
            player.Stats.UnusedPoints -= amount;
            Console.WriteLine($"Allocated {amount} point(s) to {statKey}. ({player.Stats.UnusedPoints} remaining)");
        }

    }

}
