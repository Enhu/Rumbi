using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Reflection.Metadata;

namespace Rumbi.Behaviors
{
    public class UserBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;

        public UserBehavior(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            _configuration = config;
        }

        public void Initialize()
        {
            _client.UserJoined += HandleUserJoined;
            _client.UserLeft += HandleUserLeft;
            _client.PresenceUpdated += HandleUserPresenceUpdated;
        }

        private async Task HandleUserLeft(SocketGuild guild, SocketUser user)
        {
            var modsChannel = _client.GetChannel(_configuration.GetValue<ulong>("ModChannelId")) as SocketTextChannel;
            await modsChannel.SendMessageAsync("User: username: " + user.Username + " left.");
        }
        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            var modsChannel = _client.GetChannel(_configuration.GetValue<ulong>("ModChannelId")) as SocketTextChannel;
            await modsChannel.SendMessageAsync("User: username: " + arg.Username + " joined.");
        }

        private async Task HandleUserPresenceUpdated(SocketUser user, SocketPresence oldPresence, SocketPresence newPresence)
        {
            foreach (var activity in newPresence.Activities)
            {
                Console.WriteLine(activity.Name);
            }
        }
    }
}
