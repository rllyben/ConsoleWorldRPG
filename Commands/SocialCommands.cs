namespace ConsoleWorldRPG.Commands
{
    public static class SocialCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "friend list")
            {
                ShowFriends();
                return true;
            }
            if (input == "friend requests")
            {
                ShowFriendRequests();
                return true;
            }
            if (input.StartsWith("friend add "))
            {
                AddFriend(input[11..].Trim());
                return true;
            }
            if (input.StartsWith("friend accept "))
            {
                string idStr = input[14..].Trim();
                if (int.TryParse(idStr, out int id)) AcceptFriend(id);
                else Console.WriteLine("Usage: friend accept <id>");
                return true;
            }
            if (input.StartsWith("friend remove "))
            {
                string idStr = input[14..].Trim();
                if (int.TryParse(idStr, out int id)) RemoveFriend(id);
                else Console.WriteLine("Usage: friend remove <id>");
                return true;
            }
            if (input == "friend")
            {
                Console.WriteLine("Friend commands: friend list | friend requests | friend add <name> | friend accept <id> | friend remove <id>");
                return true;
            }
            return false;
        }

        private static void ShowFriends()
        {
            if (!ConsoleHubClient.IsConnected) { Console.WriteLine("Not connected to server. (offline mode)"); return; }
            var friends = ConsoleHubClient.GetFriendsAsync().GetAwaiter().GetResult();
            if (friends.Count == 0) { Console.WriteLine("Your friend list is empty."); return; }
            Console.WriteLine($"\n== Friends ({friends.Count}) ==");
            foreach (var f in friends)
            {
                string status = f.IsOnline ? "[Online]" : "[Offline]";
                Console.ForegroundColor = f.IsOnline ? ConsoleColor.Green : ConsoleColor.DarkGray;
                Console.WriteLine($"  [{f.FriendshipId}] {f.Username}  {status}");
                Console.ResetColor();
            }
        }

        private static void ShowFriendRequests()
        {
            if (!ConsoleHubClient.IsConnected) { Console.WriteLine("Not connected to server. (offline mode)"); return; }
            var reqs = ConsoleHubClient.GetFriendRequestsAsync().GetAwaiter().GetResult();
            if (reqs.Count == 0) { Console.WriteLine("No pending friend requests."); return; }
            Console.WriteLine($"\n== Pending Friend Requests ({reqs.Count}) ==");
            foreach (var r in reqs)
                Console.WriteLine($"  [{r.FriendshipId}] from {r.FromUsername}  — use 'friend accept {r.FriendshipId}'");
        }

        private static void AddFriend(string name)
        {
            if (!ConsoleHubClient.IsConnected) { Console.WriteLine("Not connected to server. (offline mode)"); return; }
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Usage: friend add <character name>"); return; }
            bool ok = ConsoleHubClient.SendFriendRequestAsync(name).GetAwaiter().GetResult();
            if (ok)
                Console.WriteLine($"Friend request sent to {name}.");
            else
                Console.WriteLine($"Could not send friend request to '{name}'. They may not exist or you are already friends.");
        }

        private static void AcceptFriend(int id)
        {
            if (!ConsoleHubClient.IsConnected) { Console.WriteLine("Not connected to server. (offline mode)"); return; }
            bool ok = ConsoleHubClient.AcceptFriendRequestAsync(id).GetAwaiter().GetResult();
            Console.WriteLine(ok ? "Friend request accepted." : "Could not accept that request. It may have expired or not exist.");
        }

        private static void RemoveFriend(int id)
        {
            if (!ConsoleHubClient.IsConnected) { Console.WriteLine("Not connected to server. (offline mode)"); return; }
            bool ok = ConsoleHubClient.RemoveFriendAsync(id).GetAwaiter().GetResult();
            Console.WriteLine(ok ? "Friend removed." : "Could not remove friend.");
        }
    }
}
