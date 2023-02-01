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

            ulong logChannelId = RumbiConfig.ChannelConfig.BotLogs;
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

            ulong logChannelId = RumbiConfig.ChannelConfig.BotLogs;
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
                var oldStreamingActivity = oldPresence.Activities?.FirstOrDefault(x => x.Type == ActivityType.Streaming) as StreamingGame;

                if (oldStreamingActivity != null)
                {
                    var url = oldStreamingActivity.Url;
                    var channelName = url.Split('/').Last();
                    Log.Information($"Old stream activity, channel name: {channelName}");

                    var accessToken = await _twitchService.Authenticate();
                    string game = await _twitchService.GetStreamGame(accessToken, channelName);

                    if (!string.IsNullOrEmpty(game))
                        return;

                    var guild = _client.GetGuild(RumbiConfig.Config.Guild);
                    var streamingRole = guild.GetRole(RumbiConfig.RoleConfig.Streaming);
                    var guildUser = guild.GetUser(user.Id);

                    if (guildUser.Roles.Any(x => x.Id == streamingRole.Id))
                    {
                        Log.Information($"Hat stream stopped. Removing streaming role...");
                        await guildUser.RemoveRoleAsync(streamingRole);
                        Log.Information($"Done.");
                    }
                }

                var streamingActivity = newPresence.Activities.FirstOrDefault(x => x.Type == ActivityType.Streaming) as StreamingGame;

                if (streamingActivity != null)
                {

                    var guild = _client.GetGuild(RumbiConfig.Config.Guild);
                    var streamingRole = guild.GetRole(RumbiConfig.RoleConfig.Streaming);
                    var guildUser = guild.GetUser(user.Id);

                    if (guildUser.Roles.Any(x => x.Id == streamingRole.Id))
                        return;

                    var url = streamingActivity.Url;
                    var channelName = url.Split('/').Last();

                    Log.Information($"New stream activity, channel name: {channelName}");

                    var accessToken = await _twitchService.Authenticate();
                    string game = await _twitchService.GetStreamGame(accessToken, channelName);

                    if (!string.Equals(game, "A Hat in Time"))
                        return;

                    Log.Information($"Hat stream found. Adding streaming role...");

                    await guildUser.AddRoleAsync(streamingRole);

                    Log.Information($"Done.");
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