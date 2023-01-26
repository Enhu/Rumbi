using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Rumbi.Data.Config;
using System.Web;
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
            if (oldPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) is StreamingGame
                streamingActivity)
            {
                var guild = _client.GetGuild(RumbiConfig.Configuration.Guild);
                var streamingRole = guild.GetRole(RumbiConfig.Configuration.Streaming);
                var guildUser = guild.GetUser(user.Id);
                await guildUser.RemoveRoleAsync(streamingRole);
            }

            streamingActivity =
                newPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) as StreamingGame;

            if (streamingActivity != null)
            {
                //needs to implement api call to twitch api
                var url = streamingActivity.Url;
                var uri = new Uri(url);
                var segments = uri.Segments;
                var finalBit = HttpUtility.UrlDecode(segments[segments.Length - 1]);
                //await _api.Helix.Users.GetUsersAsync(null, new List<string>(){finalBit});
                var guild = _client.GetGuild(RumbiConfig.Configuration.Guild);
                var streamingRole = guild.GetRole(RumbiConfig.Configuration.Streaming);
                var guildUser = guild.GetUser(user.Id);
                await guildUser.AddRoleAsync(streamingRole);
            }
        }
    }
}