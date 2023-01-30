using Discord;
using Discord.WebSocket;
using Rumbi.Data.Config;
using Rumbi.Services;
using Serilog;

namespace Rumbi.Behaviors
{
    public class UserBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly TwitchService _twitchService;

        public UserBehavior(DiscordSocketClient client, TwitchService twitchService)
        {
            _client = client;
            _twitchService = twitchService;
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

            ulong logChannelId = RumbiConfig.Config.LoggingChannel;
            var rumbiVersion = RumbiConfig.Config.Version;

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

            ulong logChannelId = RumbiConfig.Config.LoggingChannel;
            var rumbiVersion = RumbiConfig.Config.Version;

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
            try
            {
                if (oldPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) != null)
                {
                    Log.Information($"Old streaming presence found.");

                    var guild = _client.GetGuild(RumbiConfig.Config.Guild);
                    var streamingRole = guild.GetRole(RumbiConfig.Config.Streaming);
                    var guildUser = guild.GetUser(user.Id);

                    Log.Information($"Try removing streaming role...");

                    if (guildUser.Roles.Any(x => x.Id == streamingRole.Id))
                        await guildUser.RemoveRoleAsync(streamingRole);

                    Log.Information($"Removed Streaming role.");
                }

                var streamingActivity = newPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) as StreamingGame;

                if (streamingActivity != null)
                {
                    Log.Information($"New streaming presence found, url: {streamingActivity.Url}");

                    var url = streamingActivity.Url;
                    var channelName = url.Split('/').Last();

                    Log.Information($"Try gettng streaming information..");

                    var accessToken = await _twitchService.Authenticate();
                    string game = await _twitchService.GetStreamGame(accessToken, channelName);

                    Log.Information($"Stream game name: {game}");

                    if (!string.Equals(game, "A Hat in Time"))
                        return;

                    Log.Information($"Hat stream found. Try adding streaming role...");

                    var guild = _client.GetGuild(RumbiConfig.Config.Guild);
                    var streamingRole = guild.GetRole(RumbiConfig.Config.Streaming);
                    var guildUser = guild.GetUser(user.Id);
                    await guildUser.AddRoleAsync(streamingRole);

                    Log.Information($"Streaming role added.");
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