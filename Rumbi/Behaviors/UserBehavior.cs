using Discord;
using Discord.WebSocket;
using Rumbi.Data.Config;
using Rumbi.Data.Models;
using Rumbi.Services;
using Serilog;

namespace Rumbi.Behaviors
{
    public class UserBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly TwitchService _twitchService;
        private readonly RumbiConfig _config;

        public UserBehavior(
            DiscordSocketClient client,
            TwitchService twitchService,
            RumbiConfig config
        )
        {
            _client = client;
            _twitchService = twitchService;
            _config = config;
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

            ulong logChannelId = _config.ChannelConfig.Logs;
            var rumbiVersion = _config.Version;

            embed
                .WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
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

            ulong logChannelId = _config.ChannelConfig.Logs;
            var rumbiVersion = _config.Version;

            embed
                .WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .WithColor(0, 135, 245)
                .WithFooter($"ID: {user.Id} | Rumbi {rumbiVersion}", user.GetAvatarUrl())
                .WithTimestamp(DateTime.Now)
                .WithDescription($"Account created: <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>")
                .WithTitle("Member Left");

            var logChannel = _client.GetChannel(logChannelId) as SocketTextChannel;
            await logChannel.SendMessageAsync(embed: embed.Build());
        }

        private async Task HandleUserPresenceUpdated(
            SocketUser user,
            SocketPresence oldPresence,
            SocketPresence newPresence
        )
        {
            try
            {
                var streamActivity =
                    newPresence.Activities?.FirstOrDefault(x => x.Type == ActivityType.Streaming)
                    as StreamingGame;

                var guildUser = user as SocketGuildUser;

                var userHasRole = guildUser.Roles.Any(x => x.Id == _config.RoleConfig.Streaming);

                if (streamActivity != null && !userHasRole)
                {
                    var url = streamActivity.Url;
                    var channelName = url.Split('/').Last();

                    var accessToken = await _twitchService.Authenticate();
                    string game = await _twitchService.GetStreamGame(accessToken, channelName);

                    if (string.Equals(game, "A Hat in Time"))
                    {
                        Log.Information(
                            $"New Hat stream detected. Channel: {channelName}, User: {guildUser.Username}"
                        );
                        await guildUser.AddRoleAsync(_config.RoleConfig.Streaming);
                        Log.Information("Streaming role added.");
                    }
                }

                if (streamActivity == null && userHasRole)
                {
                    Log.Information($"Hat stream stopped. User: {guildUser.Username}");
                    await guildUser.RemoveRoleAsync(_config.RoleConfig.Streaming);
                    Log.Information("Streaming role removed.");
                }
            }
            catch (Exception e)
            {
                Log.Error("An error ocurred");
                Log.Error(e, e.Message, e.InnerException);
            }
        }
    }
}
