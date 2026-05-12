namespace ConsoleWorldRPG.Commands
{
    public static class PartyCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "party")
            {
                ShowParty();
                return true;
            }

            if (input.StartsWith("party "))
            {
                string sub = input[6..].Trim();
                HandleSub(sub);
                return true;
            }

            return false;
        }

        private static void ShowParty()
        {
            if (!ConsoleHubClient.IsConnected)
            {
                Console.WriteLine("Not connected to server. (offline mode)");
                return;
            }

            var members = ConsoleHubClient.CurrentPartyMembers;
            if (members.Count == 0)
            {
                Console.WriteLine("You are not in a party.");
                return;
            }

            Console.WriteLine($"\n== Party ({members.Count} members) ==");
            foreach (var m in members)
            {
                string tag = m == ConsoleHubClient.CurrentPartyLeader ? " [Leader]" : "";
                Console.WriteLine($"  - {m}{tag}");
            }
        }

        private static void HandleSub(string sub)
        {
            if (!ConsoleHubClient.IsConnected)
            {
                Console.WriteLine("Not connected to server. (offline mode)");
                return;
            }

            if (sub.StartsWith("invite "))
            {
                string username = sub[7..].Trim();
                _ = ConsoleHubClient.InviteToPartyAsync(username);
                Console.WriteLine($"Party invite sent to {username}.");
            }
            else if (sub.StartsWith("accept "))
            {
                string partyId = sub[7..].Trim();
                _ = ConsoleHubClient.AcceptPartyInviteAsync(partyId);
            }
            else if (sub.StartsWith("decline "))
            {
                string[] parts = sub[8..].Trim().Split(' ', 2);
                string partyId = parts[0];
                string from    = parts.Length > 1 ? parts[1] : "";
                _ = ConsoleHubClient.DeclinePartyInviteAsync(partyId, from);
                Console.WriteLine("Party invite declined.");
            }
            else if (sub == "leave")
            {
                _ = ConsoleHubClient.LeavePartyAsync();
                Console.WriteLine("You left the party.");
            }
            else
            {
                Console.WriteLine("Party commands: party | party invite <player> | party accept <id> | party decline <id> <from> | party leave");
            }
        }
    }
}
