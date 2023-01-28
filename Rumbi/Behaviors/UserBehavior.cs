using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Rumbi.Data.Config;
using Serilog;
using System.Net.Http.Headers;
using TwitchLib.Api;

namespace Rumbi.Behaviors
{
    public class UserBehavior
    {
        private readonly DiscordSocketClient _client;
        private TwitchAPI _twitchApi;

        public UserBehavior(DiscordSocketClient client, TwitchAPI twitchAPI)
        {
            _client = client;
            _twitchApi = twitchAPI;
        }

        public void Initialize()
        {
            _client.UserJoined += HandleUserJoined;
            _client.UserLeft += HandleUserLeft;
            _client.PresenceUpdated += HandleUserPresenceUpdated;
        }

        private async Task HandleUserJoined(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();

            ulong logChannelId = RumbiConfig.Configuration.LoggingChannel;
            var rumbiVersion = RumbiConfig.Configuration.Version;

            embed.WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .WithColor(0, 135, 245)
                .WithFooter($"ID: {user.Id} | Rumbi {rumbiVersion}", user.GetAvatarUrl())
                .WithTimestamp(DateTime.Now)
                .WithDescription($"Account created: <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>")
                .WithTitle("Member Joined");

            var logChannel = _client.GetChannel(logChannelId) as SocketTextChannel;
            await logChannel.SendMessageAsync(embed: embed.Build());
        }

        private async Task HandleUserLeft(SocketGuild guild, SocketUser user)
        {
            var embed = new EmbedBuilder();

            ulong logChannelId = RumbiConfig.Configuration.LoggingChannel;
            var rumbiVersion = RumbiConfig.Configuration.Version;

            embed.WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .WithColor(0, 135, 245)
                .WithFooter($"ID: {user.Id} | Rumbi {rumbiVersion}", user.GetAvatarUrl())
                .WithTimestamp(DateTime.Now)
                .WithDescription($"Account created: <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>")
                .WithTitle("Member Left");


            var logChannel = _client.GetChannel(logChannelId) as SocketTextChannel;
            await logChannel.SendMessageAsync(embed: embed.Build());
        }

        private async Task HandleUserPresenceUpdated(SocketUser user, SocketPresence oldPresence,
            SocketPresence newPresence)
        {

            if (oldPresence.Activities != null && oldPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) != null)
            {
                var guild = _client.GetGuild(RumbiConfig.Configuration.Guild);
                var streamingRole = guild.GetRole(RumbiConfig.Configuration.Streaming);
                var guildUser = guild.GetUser(user.Id);
                await guildUser.RemoveRoleAsync(streamingRole);
            }

            var streamingActivity = newPresence.Activities != null ? null : newPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) as StreamingGame;

            if (streamingActivity != null)
            {
                var url = streamingActivity.Url;
                var channelName = url.Split('/').Last();

                try
                {
                    string game = await GetGameName(channelName);

                    if (!string.Equals(game, "A Hat in Time"))
                        return;

                    var guild = _client.GetGuild(RumbiConfig.Configuration.Guild);
                    var streamingRole = guild.GetRole(RumbiConfig.Configuration.Streaming);
                    var guildUser = guild.GetUser(user.Id);
                    await guildUser.AddRoleAsync(streamingRole);
                }
                catch (Exception e)
                {
                    Log.Error("An error ocurred");
                    Log.Error(e, e.Message, e.InnerException);
                }
            }
        }

        private async Task<string> GetGameName(string channelName)
        {
            string accessToken = string.Empty;

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://id.twitch.tv/oauth2/token"))
                {
                    request.Content = new StringContent($"client_id={RumbiConfig.Configuration.TwitchClientId}&client_secret={RumbiConfig.Configuration.TwitchSecret}&grant_type=client_credentials");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                        throw new HttpRequestException();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    TwitchResponse json = JsonConvert.DeserializeObject<TwitchResponse>(responseBody);

                    accessToken = json.AccessToken;
                }
            }

            _twitchApi.Settings.ClientId = RumbiConfig.Configuration.TwitchClientId;
            _twitchApi.Settings.AccessToken = accessToken;

            var user = await _twitchApi.Helix.Users.GetUsersAsync(logins: new List<string>() { channelName });
            var broadcasterId = user.Users.FirstOrDefault().Id;

            var channel = await _twitchApi.Helix.Channels.GetChannelInformationAsync(broadcasterId);

            return channel.Data.FirstOrDefault().GameName;
        }
    }

    public class TwitchResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]

        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]

        public string TokenType { get; set; }
    }
}