using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace ConsoleWorldRPG.Services
{
    public static class ConsoleHubClient
    {
        public static string BaseUrl { get; set; } = "http://localhost:5000";

        private static readonly HttpClient _http = new();
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions _playerOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new ItemConverter() }
        };
        private static HubConnection? _connection;
        private static string? _token;

        public static bool HasToken => _token is not null;

        public static bool IsConnected =>
            _connection?.State == HubConnectionState.Connected;

        // ── Party state ──────────────────────────────────────────────────────────
        public static List<string> CurrentPartyMembers { get; private set; } = new();
        public static string? CurrentPartyLeader { get; private set; }

        // ── Events ───────────────────────────────────────────────────────────────
        public static event Action<string, string, string>? ChatMessageReceived;   // sender, message, channel
        public static event Action<string>?               PlayerEntered;
        public static event Action<string>?               PlayerLeft;
        public static event Action<List<string>>?         RoomPlayersReceived;
        public static event Action<string, string>?       PartyInviteReceived;     // fromUsername, partyId
        public static event Action<List<string>, string?>? PartyUpdated;           // members, leader
        public static event Action?                        PartyDisbanded;

        // ── REST auth ────────────────────────────────────────────────────────────
        public static async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync($"{BaseUrl}/api/auth/login",
                    new { username, password });
                if (!resp.IsSuccessStatusCode) return false;

                var result = await resp.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts);
                if (result?.Token is null) return false;

                _token = result.Token;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
                return true;
            }
            catch { return false; }
        }

        // ── Hub connection ────────────────────────────────────────────────────────
        public static async Task ConnectAsync()
        {
            if (_token is null) return;
            if (_connection is not null) await DisconnectAsync();

            _connection = new HubConnectionBuilder()
                .WithUrl($"{BaseUrl}/hubs/game", opts =>
                {
                    opts.AccessTokenProvider = () => Task.FromResult<string?>(_token);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string, string>("ChatMessage", (sender, msg, channel) =>
                ChatMessageReceived?.Invoke(sender, msg, channel));

            _connection.On<string>("PlayerEntered", name =>
                PlayerEntered?.Invoke(name));

            _connection.On<string>("PlayerLeft", name =>
                PlayerLeft?.Invoke(name));

            _connection.On<List<string>>("RoomPlayers", players =>
                RoomPlayersReceived?.Invoke(players));

            _connection.On<string, string>("PartyInvite", (from, partyId) =>
                PartyInviteReceived?.Invoke(from, partyId));

            _connection.On<List<string>, string?>("PartyUpdated", (members, leader) =>
            {
                CurrentPartyMembers = members;
                CurrentPartyLeader  = leader;
                PartyUpdated?.Invoke(members, leader);
            });

            _connection.On("PartyDisbanded", () =>
            {
                CurrentPartyMembers.Clear();
                CurrentPartyLeader = null;
                PartyDisbanded?.Invoke();
            });

            try
            {
                await _connection.StartAsync();
            }
            catch { /* hub unavailable — game continues in single-player mode */ }
        }

        public static async Task DisconnectAsync()
        {
            if (_connection is null) return;
            try { await _connection.StopAsync(); } catch { }
            await _connection.DisposeAsync();
            _connection = null;
            _token = null;
            _http.DefaultRequestHeaders.Authorization = null;
            CurrentPartyMembers.Clear();
            CurrentPartyLeader = null;
        }

        // ── Hub invocations ───────────────────────────────────────────────────────
        public static async Task SetCharacterNameAsync(string characterName)
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("SetCharacterName", characterName); } catch { }
        }

        public static async Task JoinRoomAsync(int roomId)
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("JoinRoom", roomId); } catch { }
        }

        public static async Task SendMessageAsync(string message, string channel = "room", string? target = null)
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("SendMessage", message, channel, target); } catch { }
        }

        public static async Task<bool> LoadCharacterOnServerAsync(string characterName)
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<bool>("LoadCharacter", characterName); } catch { }
            return false;
        }

        public static async Task InviteToPartyAsync(string username)
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("InviteToParty", username); } catch { }
        }

        public static async Task AcceptPartyInviteAsync(string partyId)
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("AcceptPartyInvite", partyId); } catch { }
        }

        public static async Task DeclinePartyInviteAsync(string partyId, string fromUsername)
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("DeclinePartyInvite", partyId, fromUsername); } catch { }
        }

        public static async Task LeavePartyAsync()
        {
            if (IsConnected)
                try { await _connection!.InvokeAsync("LeaveParty"); } catch { }
        }

        // ── Hub combat / world actions ────────────────────────────────────────────
        public static async Task<StartCombatResult?> StartCombatAsync(int roomId)
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<StartCombatResult>("StartCombat", roomId); } catch { }
            return null;
        }

        public static async Task<CombatTurnResult?> PlayerAttackAsync()
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<CombatTurnResult>("PlayerAttack"); } catch { }
            return null;
        }

        public static async Task<CombatTurnResult?> PlayerCastSkillAsync(string skillId)
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<CombatTurnResult>("PlayerCastSkill", skillId); } catch { }
            return null;
        }

        public static async Task<GatherActionResult?> GatherAsync(int roomId)
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<GatherActionResult>("Gather", roomId); } catch { }
            return null;
        }

        public static async Task<CraftActionResult?> CraftAsync(string npcId, string recipeId, int quantity)
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<CraftActionResult>("Craft", npcId, recipeId, quantity); } catch { }
            return null;
        }

        public static async Task<UpgradeActionResult?> UpgradeAsync(string npcId, string itemId)
        {
            if (IsConnected)
                try { return await _connection!.InvokeAsync<UpgradeActionResult>("Upgrade", npcId, itemId); } catch { }
            return null;
        }

        // ── REST character operations ─────────────────────────────────────────────
        public static async Task<List<string>> GetCharacterNamesAsync()
        {
            try
            {
                var resp = await _http.GetAsync($"{BaseUrl}/api/characters");
                if (!resp.IsSuccessStatusCode) return new();
                return await resp.Content.ReadFromJsonAsync<List<string>>(_jsonOpts) ?? new();
            }
            catch { return new(); }
        }

        public static async Task<bool> SaveCharacterAsync(Player player)
        {
            try
            {
                player.CurrentRoomId = player.CurrentRoom?.Id ?? player.CurrentRoomId;
                var dataJson = JsonSerializer.Serialize(player, _playerOpts);
                var req = new
                {
                    name          = player.Name,
                    level         = player.Level,
                    experience    = player.Experience,
                    currentRoomId = player.CurrentRoomId,
                    dataJson
                };
                var resp = await _http.PostAsJsonAsync($"{BaseUrl}/api/characters", req, _jsonOpts);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public static async Task<Player?> LoadCharacterAsync(string name)
        {
            try
            {
                var resp = await _http.GetAsync(
                    $"{BaseUrl}/api/characters/{Uri.EscapeDataString(name)}");
                if (!resp.IsSuccessStatusCode) return null;

                var dto = await resp.Content.ReadFromJsonAsync<CharacterLoadResponse>(_jsonOpts);
                if (dto is null) return null;

                var player = JsonSerializer.Deserialize<Player>(dto.DataJson, _playerOpts);
                if (player is null) return null;

                player.Level         = dto.Level;
                player.Experience    = dto.Experience;
                player.CurrentRoomId = dto.CurrentRoomId;
                player.CurrentRoom   = RoomService.AllRooms.FirstOrDefault(r => r.Id == dto.CurrentRoomId);
                player.RecalculateUnusedPoints();
                player.ValidateQuestStatuses();
                SkillFactory.UpdateSkills(player);
                SkillCombinationService.ResolveCombinedSkills(player);
                SkillSlotService.ResolveSlots(player);
                SkillSlotService.MigrateIfEmpty(player);
                return player;
            }
            catch { return null; }
        }

        private record AuthResponse(string Token, string Username, DateTime ExpiresAt);
        private record CharacterLoadResponse(string DataJson, int Level, long Experience, int CurrentRoomId);
    }
}
